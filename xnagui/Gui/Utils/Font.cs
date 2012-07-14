using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ruminate.Utils {

    public class Font {

        public Texture2D FontTexture { get; private set; }

        public List<Rectangle> GlyphData { get; private set; }
        public List<Rectangle> CroppingData { get; private set; }
        public List<Char> CharactorMap { get; private set; }
        public List<Vector3> Kerning { get; private set; }

        public int LineSpacing {
            get { return SpriteFont.LineSpacing; }
            set { SpriteFont.LineSpacing = value; }
        }

        public float Spacing {
            get { return SpriteFont.Spacing; }
            set { SpriteFont.Spacing = value; }
        }

        public char? DefaultCharacter {
            get { return SpriteFont.DefaultCharacter; }
            set { SpriteFont.DefaultCharacter = value; }
        }

        public ReadOnlyCollection<char> Characters {
            get { return SpriteFont.Characters; }
        }        

        public SpriteFont SpriteFont { get; set; }

        public Font(SpriteFont font) {
            SpriteFont = font;

            var fontType = typeof(SpriteFont);

            var fields = fontType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            GlyphData = (List<Rectangle>)fields[1].GetValue(SpriteFont);
            CroppingData = (List<Rectangle>)fields[2].GetValue(SpriteFont);
            CharactorMap = (List<Char>)fields[3].GetValue(SpriteFont);
            Kerning = (List<Vector3>)fields[4].GetValue(SpriteFont);
        }

        public Rectangle GetGlyphDataForCharactor(char charactor) {
            return GlyphData[GetCharactorLocation(charactor)];
        }

        public Rectangle GetCroppingDataForCharactor(char charactor) {
            return CroppingData[GetCharactorLocation(charactor)];
        }

        public Vector3 GetKerningForCharactor(char charactor) {
            return Kerning[GetCharactorLocation(charactor)];
        }

        public int GetCharactorLocation(char charactor) {
            var counter = 0;
            foreach (var c in CharactorMap) {
                if (charactor == c) {
                    return counter;
                }
                counter++;
            }

            return -1;
        }

        public Vector2 MeasureString(string text) {
            return SpriteFont.MeasureString(text);
        }

        public Vector2 MeasureString(StringBuilder text) {
            return SpriteFont.MeasureString(text);
        }

        public void AttachToTexture2D(GraphicsDevice device, Texture2D texture) {

            var minWdith = (texture.Width < FontTexture.Width) ? texture.Width : FontTexture.Width;
            var minHeight = (texture.Height < FontTexture.Height) ? texture.Height : FontTexture.Height;

            var expandX = ((texture.Width + FontTexture.Width) * minWdith);
            var expandY = ((texture.Height + FontTexture.Height) * minHeight);

            if (expandX < expandY) {
                var target = new RenderTarget2D(device, texture.Width + FontTexture.Width, minHeight);
                var batch = new SpriteBatch(device);

                device.SetRenderTarget(target);
                device.Clear(Color.Transparent);

                batch.Draw(texture, Vector2.Zero, Color.White);
                batch.Draw(FontTexture, new Vector2(texture.Width, 0), Color.White);

                batch.End();

                var colorData = new Color[expandX];
                target.GetData(colorData);
                FontTexture.SetData(colorData);
            } else {
                var target = new RenderTarget2D(device, texture.Height + FontTexture.Height, minWdith);
                var batch = new SpriteBatch(device);

                device.SetRenderTarget(target);
                device.Clear(Color.Transparent);

                batch.Draw(texture, Vector2.Zero, Color.White);
                batch.Draw(FontTexture, new Vector2(0, texture.Height), Color.White);

                batch.End();

                var colorData = new Color[expandX];
                target.GetData(colorData);
                FontTexture.SetData(colorData);
            }
        }
    }    
}
