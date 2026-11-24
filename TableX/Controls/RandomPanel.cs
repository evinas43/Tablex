using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TableX.Controls
{
    // Coloca los hijos en posiciones "aleatorias" sin que se solapen:
    // 1. Divide el área en una cuadrícula invisible de celdas del tamaño del hijo más grande.
    // 2. Baraja las celdas y pone un hijo en cada una.
    // 3. Dentro de su celda, cada hijo se desplaza un poco al azar.
    // Usa una semilla fija, así que con el mismo tamaño de área las mesas no se mueven en cada relayout.
    public class RandomPanel : Panel
    {
        public static readonly DependencyProperty SeedProperty =
            DependencyProperty.Register(nameof(Seed), typeof(int), typeof(RandomPanel),
                new FrameworkPropertyMetadata(12345, FrameworkPropertyMetadataOptions.AffectsArrange));

        public int Seed
        {
            get => (int)GetValue(SeedProperty);
            set => SetValue(SeedProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var infinite = new Size(double.PositiveInfinity, double.PositiveInfinity);
            double maxW = 0, maxH = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(infinite);
                maxW = Math.Max(maxW, child.DesiredSize.Width);
                maxH = Math.Max(maxH, child.DesiredSize.Height);
            }

            // Ocupa todo el espacio disponible; si es infinito, lo justo para una fila
            return new Size(
                double.IsInfinity(availableSize.Width) ? maxW * InternalChildren.Count : availableSize.Width,
                double.IsInfinity(availableSize.Height) ? maxH : availableSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = InternalChildren.Count;
            if (count == 0)
                return finalSize;

            // Tamaño de celda = el hijo más grande
            double itemW = 0, itemH = 0;
            foreach (UIElement child in InternalChildren)
            {
                itemW = Math.Max(itemW, child.DesiredSize.Width);
                itemH = Math.Max(itemH, child.DesiredSize.Height);
            }

            if (itemW <= 0 || itemH <= 0)
                return finalSize;

            // Cuántas celdas caben
            int cols = Math.Max(1, (int)(finalSize.Width / itemW));
            int rows = Math.Max(1, (int)(finalSize.Height / itemH));

            // Si no caben todas, se hace la cuadrícula justa para todas (las celdas quedan más pequeñas
            // que la mesa y alguna se puede pisar: es el único caso en que hay solapes)
            if (cols * rows < count)
            {
                cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count * finalSize.Width / Math.Max(1, finalSize.Height))));
                rows = (int)Math.Ceiling(count / (double)cols);
            }

            double cellW = finalSize.Width / cols;
            double cellH = finalSize.Height / rows;

            // Baraja las celdas (Fisher-Yates) con la semilla fija
            var random = new Random(Seed);
            var cells = new List<int>(cols * rows);
            for (int i = 0; i < cols * rows; i++)
                cells.Add(i);

            for (int i = cells.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (cells[i], cells[j]) = (cells[j], cells[i]);
            }

            // Un hijo por celda, con un desplazamiento al azar dentro de ella
            for (int i = 0; i < count; i++)
            {
                var child = InternalChildren[i];
                int col = cells[i] % cols;
                int row = cells[i] / cols;

                double slackX = Math.Max(0, cellW - child.DesiredSize.Width);
                double slackY = Math.Max(0, cellH - child.DesiredSize.Height);

                double x = col * cellW + random.NextDouble() * slackX;
                double y = row * cellH + random.NextDouble() * slackY;

                // Nunca se sale del área (solo puede pasar cuando las celdas son más pequeñas que la mesa)
                x = Math.Max(0, Math.Min(x, finalSize.Width - child.DesiredSize.Width));
                y = Math.Max(0, Math.Min(y, finalSize.Height - child.DesiredSize.Height));

                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
            }

            return finalSize;
        }
    }
}
