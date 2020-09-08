// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Collections
{
    public class DrawableCollectionListItem : OsuRearrangeableListItem<BeatmapCollection>
    {
        private const float item_height = 35;

        private const float button_width = item_height * 0.75f;

        public DrawableCollectionListItem(BeatmapCollection item)
            : base(item)
        {
        }

        protected override Drawable CreateContent() => new ItemContent(Model);

        private class ItemContent : CircularContainer
        {
            private readonly BeatmapCollection collection;

            private ItemTextBox textBox;

            public ItemContent(BeatmapCollection collection)
            {
                this.collection = collection;

                RelativeSizeAxes = Axes.X;
                Height = item_height;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Children = new Drawable[]
                {
                    new DeleteButton(collection)
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        IsTextBoxHovered = v => textBox.ReceivePositionalInputAt(v)
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Right = button_width },
                        Children = new Drawable[]
                        {
                            textBox = new ItemTextBox
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = Vector2.One,
                                CornerRadius = item_height / 2,
                                Current = collection.Name
                            },
                        }
                    },
                };
            }
        }

        private class ItemTextBox : OsuTextBox
        {
            protected override float LeftRightPadding => item_height / 2;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundUnfocused = colours.GreySeafoamDarker.Darken(0.5f);
                BackgroundFocused = colours.GreySeafoam;
            }
        }

        public class DeleteButton : CompositeDrawable
        {
            public Func<Vector2, bool> IsTextBoxHovered;

            [Resolved(CanBeNull = true)]
            private DialogOverlay dialogOverlay { get; set; }

            [Resolved]
            private BeatmapCollectionManager collectionManager { get; set; }

            private readonly BeatmapCollection collection;

            private Drawable background;

            public DeleteButton(BeatmapCollection collection)
            {
                this.collection = collection;
                RelativeSizeAxes = Axes.Y;

                Width = button_width + item_height / 2; // add corner radius to cover with fill

                Alpha = 0.1f;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Red
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.Centre,
                        X = -button_width * 0.6f,
                        Size = new Vector2(10),
                        Icon = FontAwesome.Solid.Trash
                    }
                };
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) && !IsTextBoxHovered(screenSpacePos);

            protected override bool OnHover(HoverEvent e)
            {
                this.FadeTo(1f, 100, Easing.Out);
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.FadeTo(0.1f, 100);
            }

            protected override bool OnClick(ClickEvent e)
            {
                background.FlashColour(Color4.White, 150);

                if (collection.Beatmaps.Count == 0)
                    deleteCollection();
                else
                    dialogOverlay?.Push(new DeleteCollectionDialog(collection, deleteCollection));

                return true;
            }

            private void deleteCollection() => collectionManager.Collections.Remove(collection);
        }
    }
}
