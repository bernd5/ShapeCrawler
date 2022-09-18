﻿using ShapeCrawler.AutoShapes;
using ShapeCrawler.Exceptions;
using ShapeCrawler.Placeholders;
using ShapeCrawler.Shapes;
using ShapeCrawler.Shared;
using ShapeCrawler.SlideMasters;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.Tables
{
    internal class SCTableCell : ITableCell, ITextFrameContainer
    {
        private readonly ResettableLazy<TextFrame> textBox;
        private readonly bool isRemoved;

        internal SCTableCell(SCTableRow tableRow, A.TableCell aTableCell, int rowIndex, int columnIndex)
        {
            this.ParentTableRow = tableRow;
            this.ATableCell = aTableCell;
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.textBox = new ResettableLazy<TextFrame>(this.GetTextBox);
        }

        public bool IsMergedCell => this.DefineWhetherCellIsMerged();

        public SCSlideMaster SlideMasterInternal => this.ParentTableRow.ParentTable.SlideMasterInternal;

        public IPlaceholder Placeholder => throw new System.NotImplementedException();

        public IShape Shape => this.ParentTableRow.ParentTable;

        public ITextFrame TextFrame => this.textBox.Value;

        internal A.TableCell ATableCell { get; init; }

        internal int RowIndex { get; }

        internal int ColumnIndex { get; }

        private SCTableRow ParentTableRow { get; }

        public void ThrowIfRemoved()
        {
            if (this.isRemoved)
            {
                throw new ElementIsRemovedException("Table Cell was removed.");
            }

            this.ParentTableRow.ThrowIfRemoved();
        }

        private TextFrame GetTextBox()
        {
            return new TextFrame(this, this.ATableCell.TextBody!);
        }

        private bool DefineWhetherCellIsMerged()
        {
            return this.ATableCell.GridSpan != null ||
                   this.ATableCell.RowSpan != null ||
                   this.ATableCell.HorizontalMerge != null ||
                   this.ATableCell.VerticalMerge != null;
        }
    }
}