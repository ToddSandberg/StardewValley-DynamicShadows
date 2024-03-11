using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;

namespace YourProjectName
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        // TODO is there a variable already avaiable?
        int currentTime = -1;
        bool extended = false;
            
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {            
            helper.GameContent.InvalidateCache("LooseSprites/shadow");

            helper.Events.GameLoop.TimeChanged += this.OnTimeChange;
            helper.Events.Content.AssetRequested += this.OnAssetRequest;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }

        private void OnTimeChange(object sender, TimeChangedEventArgs e)
        {
            currentTime = e.NewTime;
            this.Helper.GameContent.InvalidateCache("LooseSprites/shadow");

        }

        private void OnAssetRequest(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.Name.Contains("LooseSprites"))
            {
                this.Monitor.Log($"Asset Requested: {e.NameWithoutLocale}", LogLevel.Debug);
            }
            if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/shadow"))
            {
                e.Edit(asset =>
                {
                    // TODO only do this outside
                    var editor = asset.AsImage();

                    if (!extended)
                    {
                        int tempWidth = editor.Data.Width * 200;
                        int tempHeight = editor.Data.Height * 200;
                        editor.ExtendImage(tempWidth, tempHeight);
                    }

                    IRawTextureData image = this.Helper.ModContent.Load<IRawTextureData>("assets/shadow.png");
                    int currentPixelCount = image.Width * image.Height;

                    // Calculate what the scaling factor should be based on the current time
                    float scalingFactor;
                    float time = (float)currentTime;
                    if (currentTime >= 600)
                    {
                        scalingFactor = 3f * time / 700f - 32f / 7f;
                    } else {
                        scalingFactor = 1;
                    }

                    // Create new array with correct size based on scaled
                    int newWidth = (int) Math.Floor(image.Width * scalingFactor);
                    int newPixelCount = Math.Abs(newWidth) * image.Height;
                    Color[] newData = new Color[newPixelCount];
                    // bit of a cheeky way to allow for any size image, pulling in a 500x500 image and using what i need
                    IRawTextureData newImage = this.Helper.ModContent.Load<IRawTextureData>("assets/blank.png");

                    // Below is a simple scaling algo
                    for (int i = 0; i < newPixelCount; i++)
                    {
                        // Calculate original image index
                        int originalIndex = (int)(i / Math.Abs(scalingFactor));

                        // Idk if this happens but just in case
                        if (originalIndex > currentPixelCount - 1) continue;

                        Color color = image.Data[originalIndex];
                        if (color.A == 0) continue; // ignore transparent color

                        int rowNum = originalIndex / image.Width;
                        int currentRowStart = rowNum * newImage.Width;
                        int index = currentRowStart + (i % newWidth);
                        newImage.Data[index] = color;
                    }

                    int offset = (int)Math.Floor(Math.Abs(newWidth) * 0.8);
                    int widthAddition = newWidth > 0 ? offset : newWidth + offset;
                    Rectangle areaOfOriginal = new Rectangle(editor.Data.Width / 2 - widthAddition, editor.Data.Height / 2 - (image.Height / 2), newImage.Width, newImage.Height);
                    Rectangle areaOfOverwrite = new Rectangle(0, 0, newImage.Width, newImage.Height);
                    editor.PatchImage(newImage, sourceArea: areaOfOverwrite, targetArea: areaOfOriginal);
                });
            }
        }
    }
}