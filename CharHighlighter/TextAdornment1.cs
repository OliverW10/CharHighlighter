using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace CharHighlighter
{
    /// <summary>
    /// TextAdornment1 places red boxes behind all the "a"s in the editor window
    /// </summary>
    internal sealed class TextAdornment1
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView view;

        /// <summary>
        /// Adornment brush.
        /// </summary>
        private readonly Brush brush;

        /// <summary>
        /// Adornment pen.
        /// </summary>
        private readonly Pen pen;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAdornment1"/> class.
        /// </summary>
        /// <param name="view">Text view to create the adornment for</param>
        public TextAdornment1(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            this.layer = view.GetAdornmentLayer("TextAdornment1");

            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;

            // Create the pen and brush to color the box behind the a's
            this.brush = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            this.brush.Freeze();

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            this.pen = new Pen(penBrush, 0.5);
            this.pen.Freeze();
        }

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line);
            }
        }

        char CharFromHex(string hexString)
        {
            // backwards beacuse unicode expects little endian
            byte[] b = { Convert.ToByte(hexString.Substring(2, 2), 16), Convert.ToByte(hexString.Substring(0, 2), 16) };
            return Encoding.Unicode.GetChars(b).First();
        }

        private void CreateVisuals(ITextViewLine line)
        {
            try
            {
                var charsUndecoded = GeneralOptions.Instance.Characters.Split(',');
                var chars = charsUndecoded.Select(c => {
                    return c.Length == 1 ? c[0] : CharFromHex(c.Replace("\\u", ""));
                });
                IWpfTextViewLineCollection textViewLines = this.view.TextViewLines;

                // Will only work for single unit (2 byte) utf 16 code points
                for (int charIndex = line.Start; charIndex < line.End; charIndex++)
                {
                    if (chars.Contains(this.view.TextSnapshot[charIndex]))
                    {
                        SnapshotSpan span = new SnapshotSpan(this.view.TextSnapshot, Span.FromBounds(charIndex, charIndex + 1));
                        Geometry geometry = textViewLines.GetMarkerGeometry(span);
                        if (geometry != null)
                        {
                            var drawing = new GeometryDrawing(this.brush, this.pen, geometry);
                            drawing.Freeze();

                            var drawingImage = new DrawingImage(drawing);
                            drawingImage.Freeze();

                            var image = new Image
                            {
                                Source = drawingImage,
                            };

                            // Align the image with the top of the bounds of the text geometry
                            Canvas.SetLeft(image, geometry.Bounds.Left);
                            Canvas.SetTop(image, geometry.Bounds.Top);

                            this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
                        }
                    }
                }
            } catch (Exception)
            {
                // yeah idk
            }
        }
    }
}
