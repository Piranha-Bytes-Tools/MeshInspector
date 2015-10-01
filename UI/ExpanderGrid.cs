using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MeshInspector.UI
{
    /// <summary>
    /// extended grid control that handles expanders correctly
    /// found in internet to correctly resize the grid rows if an expander is collapes and expanded
    /// </summary>
    public class ExpanderGrid : Grid
    {
        private readonly Dictionary<UIElement, double> m_ElementSizes;

        public ExpanderGrid()
        {
            this.m_ElementSizes = new Dictionary<UIElement, double>();
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (Expander child in this.Children.OfType<Expander>().Where(expander => !expander.IsExpanded))
            {
                if (child.ExpandDirection == ExpandDirection.Down || child.ExpandDirection == ExpandDirection.Up)
                {
                    int row = Grid.GetRow(child);
                    if (!this.RowDefinitions.Any())
                        continue;

                    this.RowDefinitions[row].Height = new GridLength(0, GridUnitType.Auto);
                }
                else
                {
                    int column = Grid.GetColumn(child);
                    if (!this.ColumnDefinitions.Any())
                        continue;

                    this.ColumnDefinitions[column].Width = new GridLength(0, GridUnitType.Auto);
                }
            }

            return base.ArrangeOverride(arrangeSize);
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            Expander expander = child as Expander;
            if (expander != null)
            {
                bool verticalExpansion = expander.ExpandDirection == ExpandDirection.Down || expander.ExpandDirection == ExpandDirection.Up;

                int row = Grid.GetRow(expander);
                int column = Grid.GetColumn(expander);

                if (expander.IsExpanded)
                {
                    double oldSize;
                    if (!this.m_ElementSizes.TryGetValue(child, out oldSize))
                        oldSize = 1;

                    GridLength gridLength = new GridLength(oldSize, GridUnitType.Star);
                    
                    if (verticalExpansion)
                        this.RowDefinitions[row].Height = gridLength;
                    else
                        this.ColumnDefinitions[column].Width = gridLength;
                }
                else
                {
                    GridLength gridLength = new GridLength(0, GridUnitType.Auto);
                    
                    if (verticalExpansion)
                    {
                        this.m_ElementSizes[child] = this.RowDefinitions[row].Height.Value;
                        this.RowDefinitions[row].Height = gridLength;
                    }
                    else
                    {
                        this.m_ElementSizes[child] = this.ColumnDefinitions[column].Width.Value;
                        this.ColumnDefinitions[column].Width = gridLength;
                    }
                }
            }

            base.OnChildDesiredSizeChanged(child);
        }
    }
}
