"""Aspect roll normalisation. No network: every fixture is a literal payload."""

from __future__ import annotations

from diablotrade.models import SEARCH_ID_CAP, Item, SavedSearch


def make(equipment_type: str, roll: float | None) -> Item:
    effect: dict[str, object] | None = (
        {"attributeId": "aspect", "values": [roll]} if roll is not None else None
    )
    return Item.parse({"equipmentType": equipment_type, "effectAffix": effect})


class TestAspectRoll:
    def test_unscaled_slot_reports_the_shown_roll_as_base(self) -> None:
        item = make("CHEST", 60.0)
        assert item.aspect_roll == 60.0
        assert item.aspect_scale == 1.0
        assert item.base_aspect_roll == 60.0

    def test_amulet_roll_is_divided_by_its_1_5x_range(self) -> None:
        # 90 on an amulet is the same base roll as 60 on a chest, so ranking on
        # the shown number alone silently promotes every amulet.
        assert make("AMULET", 90.0).base_aspect_roll == 60.0

    def test_two_handed_roll_is_halved(self) -> None:
        assert make("TWO_HANDED_SWORD", 120.0).base_aspect_roll == 60.0
        assert make("POLEARM", 120.0).base_aspect_roll == 60.0

    def test_ambiguous_slot_yields_no_base_roll(self) -> None:
        # One "SWORD" label covers both the 1x and the 2x form, so the scale is
        # not knowable from the payload. Skipped, never guessed.
        item = make("SWORD", 120.0)
        assert item.aspect_roll == 120.0
        assert item.aspect_scale is None
        assert item.base_aspect_roll is None

    def test_item_without_an_aspect_has_no_roll(self) -> None:
        item = make("CHEST", None)
        assert item.aspect_roll is None
        assert item.base_aspect_roll is None

    def test_unknown_slot_falls_back_to_unscaled(self) -> None:
        assert make("SOMETHING_NEW", 55.0).base_aspect_roll == 55.0


class TestSearchTruncation:
    def test_a_short_result_is_not_truncated(self) -> None:
        search = SavedSearch.parse({"id": "abc", "listings": ["a", "b"]})
        assert not search.is_truncated

    def test_hitting_the_cap_is_reported_as_truncated(self) -> None:
        # The endpoint has no pagination, so a capped result is a floor rather
        # than a count - silently treating it as a total is the bug this guards.
        ids = [str(n) for n in range(SEARCH_ID_CAP)]
        assert SavedSearch.parse({"id": "abc", "listings": ids}).is_truncated
