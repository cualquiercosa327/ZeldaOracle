﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Xna.Framework.Content;
using ZeldaEditor.Control;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Translation;
using ZeldaOracle.Game;
using ZeldaOracle.Game.Tiles;

namespace ZeldaEditor.WinForms {
	public class TextMessageDisplay : GraphicsDeviceControl {

		// The states the text reader can be in.
		private enum TextReaderState {
			WritingLine,
			PushingLine,
			PressToContinue,
			PressToEndParagraph,
			Finished
		}

		private static ContentManager content;
		private static Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch;
		private DispatcherTimer dispatcherTimer;
		
		// The wrapped and formatted lines.
		private WrappedLetterString wrappedString;
		// The timer for the transitions.
		private int timer;
		// The number of lines to use for this window.
		private int linesPerWindow;
		// The lines left before the next set of lines is in the message box.
		private int windowLinesLeft;
		// The current window line of the line being written.
		private int windowLine;
		// The current line in the wrapped string.
		private int currentLine;
		// The current character of the current line.
		private int currentChar;
		// The current state of the text reader.
		private TextReaderState state;
		// The timer used to update the arrow sprite.
		private int arrowTimer;

		private Stopwatch watch;

		private int lastTicks;

		private EditorControl editorControl;

		private bool enterPressed;

		public event EventHandler MessageFinished;

		public EditorControl EditorControl {
			get { return editorControl; }
			set {
				editorControl = value;
				editorControl.UpdateTicks();
				lastTicks = editorControl.Ticks;
			}
		}

		public TextMessageDisplay(string message) {
			this.wrappedString      = GameData.FONT_LARGE.WrapString(message, 128);
			this.enterPressed       = false;

			this.linesPerWindow     = 2;

			Restart();

			Rectangle2I bounds = BoxBounds;
			Width = bounds.Width;
			Height = bounds.Height;

			watch = new Stopwatch();
		}

		public void UpdateMessage(string message, int caretPosition) {
			int caretLine;
			this.wrappedString      = GameData.FONT_LARGE.WrapString(message, 128, caretPosition, out caretLine);

			Restart();
			if (caretLine == 0)
				currentLine = Math.Min(1, wrappedString.LineCount - 1);
			else
				currentLine = Math.Max(0, caretLine);
			windowLine = GMath.Clamp(wrappedString.LineCount - 1, 0, 1);

			if (currentLine > 0 && wrappedString.Lines[currentLine - 1].LastOrDefault().Char == FormatCodes.ParagraphCharacter) {
				if (currentLine + 1 < wrappedString.LineCount)
					currentLine++;
				else
					windowLine = 0;
			}

			if (currentLine + 1 >= wrappedString.LineCount)
				state = TextReaderState.Finished;
			else if (wrappedString.Lines[currentLine].LastOrDefault().Char == FormatCodes.ParagraphCharacter)
				state = TextReaderState.PressToEndParagraph;
			else
				state = TextReaderState.PressToContinue;

			if (caretLine != -1)
				currentChar = wrappedString.Lines[currentLine].Length;

			if (state == TextReaderState.Finished) {
				MessageFinished(this, new EventArgs());
			}
		}

		public void Restart() {
			this.timer              = 0;
			this.arrowTimer         = 0;
			this.enterPressed       = false;

			this.windowLinesLeft    = this.linesPerWindow;
			this.windowLine         = 0;
			this.currentLine        = 0;
			this.currentChar        = 0;
			this.state              = TextReaderState.WritingLine;

			if (this.wrappedString.LineCount == 0) {
				this.state = TextReaderState.Finished;
				MessageFinished(this, new EventArgs());
			}
		}

		protected override void Initialize() {
			content     = new ContentManager(Services, "Content");
			spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);

			//KeyDown += OnKeyDown;

			this.ResizeRedraw = true;

			// Start the timer to refresh the panel.
			dispatcherTimer = new DispatcherTimer(
				TimeSpan.FromMilliseconds(15),
				DispatcherPriority.Render,
				delegate { Invalidate(); },
				System.Windows.Application.Current.Dispatcher);
			watch.Start();
		}

		public void EnterPressed() {
			enterPressed = true;
		}

		/*private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter && Focused) {
				enterPressed = true;
			}
		}*/

		protected override void Dispose(bool disposing) {
			if (disposing) {
				content.Unload();
			}

			base.Dispose(disposing);
		}


		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		// The default color used by the text reader.
		private readonly Color TextColor = new Color(248, 208, 136);

		private void Update(int count) {
			for (int i = 0; i < count; i++) {
				if (timer > 0 && (state != TextReaderState.WritingLine || !enterPressed)) {
					timer -= 1;
				}
				else {
					switch (state) {
					case TextReaderState.WritingLine:

						if (currentChar < wrappedString.Lines[currentLine].Length) {
							char c = wrappedString.Lines[currentLine][currentChar].Char;
							bool isLetter = (c != ' ');

							currentChar++;
							if (CheckAndConsumeEnter()) {
								currentChar = wrappedString.Lines[currentLine].Length;
							}
						}

						if (currentChar >= wrappedString.Lines[currentLine].Length) {
							windowLinesLeft--;
							if (currentLine + 1 == wrappedString.LineCount) {
								state = TextReaderState.Finished;
								MessageFinished(this, new EventArgs());
							}
							else if (wrappedString.Lines[currentLine].EndsWith(FormatCodes.ParagraphCharacter)) {
								state = TextReaderState.PressToEndParagraph;
								arrowTimer = 0;
							}
							else if (windowLinesLeft == 0) {
								state = TextReaderState.PressToContinue;
								arrowTimer = 0;
							}
							else if (windowLine + 1 < linesPerWindow) {
								windowLine++;
								currentLine++;
								currentChar = 0;
							}
							else {
								currentLine++;
								currentChar = 0;
								state = TextReaderState.PushingLine;
								timer = 4;
							}
						}
						else {
							timer = 1;
						}
						break;

					case TextReaderState.PushingLine:
						state = TextReaderState.WritingLine;
						break;

					case TextReaderState.PressToContinue:
						arrowTimer++;
						if (arrowTimer == 32)
							arrowTimer = 0;
						if (CheckAndConsumeEnter()) {
							state = TextReaderState.PushingLine;
							timer = 4;
							windowLinesLeft = linesPerWindow;
							currentChar = 0;
							currentLine++;
						}
						break;
					case TextReaderState.PressToEndParagraph:
						arrowTimer++;
						if (arrowTimer == 32)
							arrowTimer = 0;
						if (CheckAndConsumeEnter()) {
							state = TextReaderState.WritingLine;
							windowLinesLeft = linesPerWindow;
							currentChar = 0;
							windowLine = 0;
							currentLine++;
						}
						break;
					case TextReaderState.Finished:
						// TODO: Switch to any key
						if (CheckAndConsumeEnter()) {
							Restart();
						}
						break;
					}
				}
			}
			enterPressed = false;
		}

		private bool CheckAndConsumeEnter() {
			if (enterPressed) {
				enterPressed = false;
				return true;
			}
			return false;
		}

		private Rectangle2I BoxBounds {
			get { return new Rectangle2I(new Point2I(144, 8 + 16 * linesPerWindow)); }
		}

		protected override void Draw() {
			Graphics2D g = new Graphics2D(spriteBatch);
			g.Begin(GameSettings.DRAW_MODE_DEFAULT);
			g.Clear(Color.White);

			int currentTicks = (int)(watch.Elapsed.TotalSeconds * 60);
			Update(Math.Min(60, currentTicks - lastTicks));
			lastTicks = currentTicks;

			Point2I pos = Point2I.Zero;

			Rectangle2I bounds = BoxBounds;
			bounds.Point += pos;

			g.FillRectangle(bounds, Color.Black);

			// Draw the finished writting lines.
			for (int i = 0; i < windowLine; i++) {
				if (state == TextReaderState.PushingLine && timer >= 2)
					g.DrawLetterString(GameData.FONT_LARGE, wrappedString.Lines[currentLine - windowLine + i], pos + new Point2I(8, 6 + 16 * i + 8), TextColor);
				else
					g.DrawLetterString(GameData.FONT_LARGE, wrappedString.Lines[currentLine - windowLine + i], pos + new Point2I(8, 6 + 16 * i), TextColor);
			}
			// Draw the currently writting line.
			if (currentLine < wrappedString.LineCount)
				g.DrawLetterString(GameData.FONT_LARGE, wrappedString.Lines[currentLine].Substring(0, currentChar), pos + new Point2I(8, 6 + 16 * windowLine), TextColor);

			// Draw the next line arrow.
			if ((state == TextReaderState.PressToContinue || state ==  TextReaderState.PressToEndParagraph) && arrowTimer >= 16)
				g.DrawSprite(GameData.SPR_HUD_TEXT_NEXT_ARROW, pos + new Point2I(136, 16 * linesPerWindow));

			g.End();
		}
	}
}
