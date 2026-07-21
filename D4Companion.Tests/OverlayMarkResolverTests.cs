using D4Companion.Entities;

namespace D4Companion.Tests
{
    /// <summary>
    /// The marker shape is the overlay's only signal besides colour, and it can normally
    /// only be checked by looking at the game. OverlayHandler draws the tooltip in two
    /// passes that each carried their own copy of this ladder; these pin the shared rule.
    /// </summary>
    public class OverlayMarkResolverTests
    {
        private static OverlayMarkKind Resolve(ItemAffix affix, bool isDungeonSigil = false,
            bool minimalValueFilterEnabled = false, bool isBelowMinimalValue = false)
        {
            return OverlayMarkResolver.Resolve(affix, isDungeonSigil, minimalValueFilterEnabled, isBelowMinimalValue);
        }

        [Test]
        public void OrdinaryAffix_IsACircle()
        {
            Assert.That(Resolve(new ItemAffix()), Is.EqualTo(OverlayMarkKind.Circle));
        }

        [Test]
        public void GreaterAffix_IsATriangle()
        {
            // This is what makes an imported Greater Affix visible in game. It was never
            // reached before, because the Maxroll importer read a key the build did not set.
            Assert.That(Resolve(new ItemAffix { IsGreater = true }), Is.EqualTo(OverlayMarkKind.Triangle));
        }

        [Test]
        public void AnyTypeAffix_IsARectangleEvenWhenGreater()
        {
            // IsAnyType is checked first: the affix is not tied to this slot, and saying so
            // outranks saying the build wants it as a Greater Affix.
            Assert.That(Resolve(new ItemAffix { IsAnyType = true, IsGreater = true }), Is.EqualTo(OverlayMarkKind.Rectangle));
        }

        [Test]
        public void BelowMinimalValue_IsARectangleOnlyWhileTheFilterIsOn()
        {
            var affix = new ItemAffix();

            Assert.Multiple(() =>
            {
                Assert.That(Resolve(affix, minimalValueFilterEnabled: true, isBelowMinimalValue: true),
                    Is.EqualTo(OverlayMarkKind.Rectangle));
                Assert.That(Resolve(affix, minimalValueFilterEnabled: false, isBelowMinimalValue: true),
                    Is.EqualTo(OverlayMarkKind.Circle));
            });
        }

        [Test]
        public void GreaterAffix_OutranksTheMinimalValueRectangle()
        {
            // Order matters: a greater affix under the value threshold still draws as a
            // triangle, so the shape keeps meaning "the build wants a Greater Affix here".
            var affix = new ItemAffix { IsGreater = true };

            Assert.That(Resolve(affix, minimalValueFilterEnabled: true, isBelowMinimalValue: true),
                Is.EqualTo(OverlayMarkKind.Triangle));
        }

        [Test]
        public void DungeonSigil_OutranksEverything()
        {
            var affix = new ItemAffix { IsGreater = true, IsAnyType = true };

            Assert.That(Resolve(affix, isDungeonSigil: true), Is.EqualTo(OverlayMarkKind.SigilDungeon));
        }

        [Test]
        public void Rank_DoesNotChangeTheShape()
        {
            // The two signals are deliberately separate. Shape says what kind of affix this
            // is - ordinary, greater, or not tied to this slot - and the digit the overlay
            // draws beside it says where the build ranks it. Folding rank into the shape
            // would cost the greater-affix triangle its meaning, so a rank-1 ordinary affix
            // and a rank-7 ordinary affix must still resolve to the same mark.
            Assert.That(Resolve(new ItemAffix { Rank = 1 }), Is.EqualTo(Resolve(new ItemAffix { Rank = 7 })));
        }
    }
}
