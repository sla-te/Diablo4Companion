"""Search-creation tests. No network: the client is stubbed at its two seams."""

from __future__ import annotations

import json
from typing import final, override

import pytest

from diablotrade import search
from diablotrade.client import AuthRequiredError, Client, DiabloTradeError


@final
class StubClient(Client):
    """A Client that records the Server Action POST instead of sending it."""

    def __init__(self, response: str, *, cookie: str | None = "session=stub") -> None:
        super().__init__(cookie=cookie)
        self._response = response
        self.body = b""
        self.headers: dict[str, str] = {}
        self.posted_to = ""
        self.fetched: list[str] = []

    @override
    def fetch_text(self, path: str) -> str:
        # Only ever called to scrape the deploy hash off the search page.
        self.fetched.append(path)
        return '<html data-dpl-id="0123456789abcdef0123456789abcdef01234567">'

    @override
    def post_raw(self, path: str, body: bytes, headers: dict[str, str]) -> str:
        self.posted_to = path
        self.body = body
        self.headers = headers
        return self._response


def _fields(body: bytes) -> dict[str, str]:
    """Pull field name -> value out of the multipart body the client would send."""
    text = body.decode("utf-8")
    found: dict[str, str] = {}
    for part in text.split("\r\n--"):
        marker = 'name="'
        if marker not in part:
            continue
        name = part.split(marker, 1)[1].split('"', 1)[0]
        found[name] = part.split("\r\n\r\n", 1)[1].rsplit("\r\n", 1)[0]
    return found


RESPONSE = '0:{"a":"$@1"}\n1:"/listings/items/2fsprW"\n'


class TestCreateSearchEncoding:
    def test_sends_all_three_action_fields(self) -> None:
        client = StubClient(RESPONSE)
        search.create_search(client, search.build_filters())
        assert set(_fields(client.body)) == {"_1_input", "_1_requestId", "0"}

    def test_field_zero_is_the_argument_array(self) -> None:
        # Without this field the server decodes no arguments and the action
        # throws a 500. It is the whole reason create_search used to fail.
        client = StubClient(RESPONSE)
        search.create_search(client, search.build_filters())
        assert json.loads(_fields(client.body)["0"]) == [{"error": ""}, "$K1"]

    def test_input_field_carries_the_filters(self) -> None:
        client = StubClient(RESPONSE)
        search.create_search(client, search.build_filters(itemPowerMin="800"))
        sent = json.loads(_fields(client.body)["_1_input"])
        assert sent["itemPowerMin"] == "800"
        assert sent["listingType"] == "ITEM"

    def test_returns_the_short_id_from_the_response(self) -> None:
        client = StubClient(RESPONSE)
        assert search.create_search(client, search.build_filters()) == "2fsprW"

    def test_posts_to_the_bare_search_page(self) -> None:
        # Not /listings/items/<previousShortId>: the capture shows the browser
        # posting to the bare page even when it is showing a saved search.
        client = StubClient(RESPONSE)
        assert search.create_search(client, search.build_filters()) == "2fsprW"
        assert client.posted_to == "/listings/items"

    def test_raises_when_the_response_has_no_short_id(self) -> None:
        client = StubClient('1:E{"digest":"2245531887"}')
        with pytest.raises(DiabloTradeError, match="rotated"):
            search.create_search(client, search.build_filters())


class TestCreateSearchAuth:
    def test_anonymous_client_is_refused_before_any_request(self) -> None:
        # The site answers a cookie-less action with a 303 to /session-expired,
        # which reads as anything but "log in" - so refuse it locally instead.
        client = StubClient(RESPONSE, cookie=None)
        with pytest.raises(AuthRequiredError, match="logged-in session"):
            search.create_search(client, search.build_filters())
        assert client.body == b""


class TestBuildFilters:
    def test_unknown_key_is_rejected(self) -> None:
        with pytest.raises(ValueError, match="unknown filter keys"):
            search.build_filters(itemPowerMinimum="800")

    def test_defaults_are_left_intact_by_an_override(self) -> None:
        assert search.build_filters(priceMax="100")["priceMin"] == ""


class TestStatGroup:
    def test_or_group_carries_the_site_s_own_min_matches(self) -> None:
        group = search.stat_group(["a", "b", "c"], mode="or", min_matches="2")
        assert group["type"] == "or"
        assert group["minMatches"] == "2"
        assert group["maxMatches"] == ""

    def test_min_matches_is_rejected_on_a_non_or_group(self) -> None:
        # The site only ever attaches it to an "or" group; an "and" group with
        # minMatches would be silently ignored, which reads as "no results".
        with pytest.raises(ValueError, match="only apply to an 'or' group"):
            search.stat_group(["a", "b"], mode="and", min_matches="1")


class TestDefaultFilters:
    def test_exclude_generic_class_is_present(self) -> None:
        # Read out of the site's zod schema, not the capture - the capture was
        # taken with the control untouched so the key never appeared.
        assert search.DEFAULT_FILTERS["excludeGenericClass"] is False
