using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using ShapeCrawler.Shapes;
using ShapeCrawler.Tests.Helpers;
using ShapeCrawler.Tests.Helpers.Attributes;
using ShapeCrawler.Tests.Properties;
using Xunit;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace ShapeCrawler.Tests;

public class FontTests : ShapeCrawlerTest, IClassFixture<PresentationFixture>
{
    private readonly PresentationFixture _fixture;

    public FontTests(PresentationFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [SlideShapeData("002.pptx", 2, 3, "Palatino Linotype")]
    [SlideShapeData("001.pptx", 1, 4, "Broadway")]
    [SlideShapeData("001.pptx", 1, 7, "Calibri Light")]
    [SlideShapeData("autoshape-case015.pptx", 1, "Title 1", "Franklin Gothic Medium")]
    public void Name_Getter_returns_font_name(IShape shape, string expectedFontName)
    {
        // Arrange
        var autoShape = (IAutoShape)shape;
        var font = autoShape.TextFrame!.Paragraphs[0].Portions[0].Font;

        // Act
        var fontName = font.Name;

        // Assert
        fontName.Should().Be(expectedFontName);
    }

    [Fact]
    public void Name_Getter_returns_Calibri_Light()
    {
        // Arrange
        ITextFrame textBox = ((IAutoShape)_fixture.Pre001.Slides[4].Shapes.First(sp => sp.Id == 5)).TextFrame;

        // Act
        string portionFontName = textBox.Paragraphs[0].Portions[0].Font.Name;

        // Assert
        portionFontName.Should().BeEquivalentTo("Calibri Light");
    }

    [Theory]
    [SlideShapeData("001.pptx", 1, "TextBox 3")]
    [SlideShapeData("001.pptx", 3, "Text Placeholder 3")]
    public void Name_Setter_sets_font_name(IShape shape)
    {
        // Arrange
        var autoShape = (IAutoShape)shape;
        var font = autoShape.TextFrame!.Paragraphs[0].Portions[0].Font;

        // Act
        font.Name = "Time New Roman";

        // Assert
        font.Name.Should().Be("Time New Roman");
    }

    [Theory]
    [MasterShapeData("001.pptx", "Freeform: Shape 7", 18)]
    [SlideShapeData("020.pptx", 1, 3, 18)]
    [SlideShapeData("015.pptx", 2, 61, 18)]
    [SlideShapeData("009_table.pptx", 3, 2, 18)]
    [SlideShapeData("009_table.pptx", 4, 2, 44)]
    [SlideShapeData("009_table.pptx", 4, 3, 32)]
    [SlideShapeData("019.pptx", 1, 4103, 18)]
    [SlideShapeData("019.pptx", 1, 2, 12)]
    [SlideShapeData("014.pptx", 2, 5, 21)]
    [SlideShapeData("012_title-placeholder.pptx", 1, "Title 1", 20)]
    [SlideShapeData("010.pptx", 1, 2, 15)]
    [SlideShapeData("014.pptx", 4, 5, 12)]
    [SlideShapeData("014.pptx", 5, 4, 12)]
    [SlideShapeData("014.pptx", 6, 52, 27)]
    [SlideShapeData("autoshape-case016.pptx", 1, "Text Placeholder 1", 28)]
    public void Size_Getter_returns_font_size(IShape shape, int expectedSize)
    {
        // Arrange
        var autoShape = (IAutoShape)shape;
        var font = autoShape.TextFrame!.Paragraphs[0].Portions[0].Font;
        
        // Act
        var fontSize = font.Size;
        
        // Assert
        fontSize.Should().Be(expectedSize);
    }
    
    [Fact]
    public void Size_Getter_returns_font_size_of_non_first_portion()
    {
        // Arrange
        var font1 = _fixture.Pre015.Slides[0].Shapes.GetById<IAutoShape>(5).TextFrame!.Paragraphs[0].Portions[2].Font;
        var font2 = _fixture.Pre009.Slides[2].Shapes.GetById<IAutoShape>(2).TextFrame!.Paragraphs[0].Portions[1].Font;

        // Act
        var fontSize1 = font1.Size;
        var fontSize2 = font2.Size;
        
        // Assert
        fontSize1.Should().Be(18);
        fontSize2.Should().Be(20);
    }

    [Theory]
    [MemberData(nameof(TestCasesSizeGetter))]
    public void Size_Getter_returns_font_size_of_Placeholder(TestCase testCase)
    {
        // Arrange
        var font = testCase.AutoShape.TextFrame!.Paragraphs[0].Portions[0].Font;
        var expectedFontSize = testCase.ExpectedInt;

        // Act
        var fontSize = font.Size;

        // Assert
        fontSize.Should().Be(expectedFontSize);
    }

    public static IEnumerable<object[]> TestCasesSizeGetter
    {
        get
        {
            var testCase1 = new TestCase("#1");
            testCase1.PresentationName = "028.pptx";
            testCase1.SlideNumber = 1;
            testCase1.ShapeId = 4098;
            testCase1.ExpectedInt = 32;
            yield return new object[] { testCase1 };

            var testCase2 = new TestCase("#2");
            testCase2.PresentationName = "029.pptx";
            testCase2.SlideNumber = 1;
            testCase2.ShapeName = "Content Placeholder 2";
            testCase2.ExpectedInt = 25;
            yield return new object[] { testCase2 };
        }
    }

    [Fact]
    public void Size_Getter_returns_Font_Size_of_Non_Placeholder_Table()
    {
        // Arrange
        var table = (ITable)this._fixture.Pre009.Slides[2].Shapes.First(sp => sp.Id == 3);
        var cellPortion = table.Rows[0].Cells[0].TextFrame.Paragraphs[0].Portions[0];

        // Act-Assert
        cellPortion.Font.Size.Should().Be(18);
    }

    [Theory]
    [MemberData(nameof(TestCasesSizeSetter))]
    public void Size_Setter_sets_font_size(TestCase testCase)
    {
        // Arrange
        var pres = testCase.Presentation;
        var font = testCase.AutoShape.TextFrame!.Paragraphs[0].Portions[0].Font;
        var mStream = new MemoryStream();
        var oldSize = font.Size;
        var newSize = oldSize + 2;

        // Act
        font.Size = newSize;

        // Assert
        var errors = PptxValidator.Validate(pres);
        errors.Should().BeEmpty();
        pres.SaveAs(mStream);
        testCase.SetPresentation(mStream);
        font = testCase.AutoShape.TextFrame!.Paragraphs[0].Portions[0].Font;
        font.Size.Should().Be(newSize);
    }

    public static IEnumerable<object[]> TestCasesSizeSetter
    {
        get
        {
            var testCase1 = new TestCase("#1");
            testCase1.PresentationName = "001.pptx";
            testCase1.SlideNumber = 1;
            testCase1.ShapeName = "TextBox 3";
            yield return new object[] { testCase1 };
            
            var testCase2 = new TestCase("#2");
            testCase2.PresentationName = "026.pptx";
            testCase2.SlideNumber = 1;
            testCase2.ShapeName = "AutoShape 1";
            yield return new object[] { testCase2 };
            
            var testCase3 = new TestCase("#3");
            testCase3.PresentationName = "autoshape-case016.pptx";
            testCase3.SlideNumber = 1;
            testCase3.ShapeName = "Text Placeholder 1";
            yield return new object[] { testCase3 };
        }
    }

    [Theory]
    [SlideShapeData("#1", "001.pptx", 1, "TextBox 3")]
    [SlideShapeData("#2", "026.pptx", 1, "AutoShape 1")]
    [SlideShapeData("#3", "autoshape-case016.pptx", 1, "Text Placeholder 1")]
    public void CanChange_returns_true(string displayName, IShape shape)
    {
        // Arrange
        var autoShape = (IAutoShape)shape;
        var font = autoShape.TextFrame!.Paragraphs[0].Portions[0].Font;

        // Act
        var canChange = font.CanChange();

        // Assert
        canChange.Should().BeTrue();
    }
    
    [Fact]
    public void IsBold_GetterReturnsTrue_WhenFontOfNonPlaceholderTextIsBold()
    {
        // Arrange
        IAutoShape nonPlaceholderAutoShapeCase1 =
            (IAutoShape)_fixture.Pre020.Slides[0].Shapes.First(sp => sp.Id == 3);
        IFont fontC1 = nonPlaceholderAutoShapeCase1.TextFrame.Paragraphs[0].Portions[0].Font;

        // Act-Assert
        fontC1.IsBold.Should().BeTrue();
    }

    [Fact]
    public void IsBold_GetterReturnsTrue_WhenFontOfPlaceholderTextIsBold()
    {
        // Arrange
        IAutoShape placeholderAutoShape = (IAutoShape)_fixture.Pre020.Slides[1].Shapes.First(sp => sp.Id == 6);
        IPortion portion = placeholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act
        bool isBold = portion.Font.IsBold;

        // Assert
        isBold.Should().BeTrue();
    }

    [Fact]
    public void IsBold_GetterReturnsFalse_WhenFontOfNonPlaceholderTextIsNotBold()
    {
        // Arrange
        IAutoShape nonPlaceholderAutoShape = (IAutoShape)_fixture.Pre020.Slides[0].Shapes.First(sp => sp.Id == 2);
        IPortion portion = nonPlaceholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act
        bool isBold = portion.Font.IsBold;

        // Assert
        isBold.Should().BeFalse();
    }

    [Fact]
    public void IsBold_GetterReturnsFalse_WhenFontOfPlaceholderTextIsNotBold()
    {
        // Arrange
        IAutoShape placeholderAutoShape = (IAutoShape)_fixture.Pre020.Slides[2].Shapes.First(sp => sp.Id == 7);
        IPortion portion = placeholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act
        bool isBold = portion.Font.IsBold;

        // Assert
        isBold.Should().BeFalse();
    }

    [Fact]
    public void IsBold_Setter_AddsBoldForNonPlaceholderTextFont()
    {
        // Arrange
        var mStream = new MemoryStream();
        IPresentation presentation = SCPresentation.Open(Resources._020);
        IAutoShape nonPlaceholderAutoShape = (IAutoShape)presentation.Slides[0].Shapes.First(sp => sp.Id == 2);
        IPortion portion = nonPlaceholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act
        portion.Font.IsBold = true;

        // Assert
        portion.Font.IsBold.Should().BeTrue();
        presentation.SaveAs(mStream);
        presentation = SCPresentation.Open(mStream);
        nonPlaceholderAutoShape = (IAutoShape)presentation.Slides[0].Shapes.First(sp => sp.Id == 2);
        portion = nonPlaceholderAutoShape.TextFrame.Paragraphs[0].Portions[0];
        portion.Font.IsBold.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestCasesIsBold))]
    public void IsBold_Setter_AddsBoldForPlaceholderTextFont(TestElementQuery portionQuery)
    {
        // Arrange
        MemoryStream mStream = new();
        var portion = portionQuery.GetParagraphPortion();
        var pres = portionQuery.Presentation;

        // Act
        portion.Font.IsBold = true;

        // Assert
        portion.Font.IsBold.Should().BeTrue();

        pres.SaveAs(mStream);
        pres = SCPresentation.Open(mStream);
        portionQuery.Presentation = pres;
        portion = portionQuery.GetParagraphPortion();
        portion.Font.IsBold.Should().BeTrue();
    }

    public static IEnumerable<object[]> TestCasesIsBold()
    {
        TestElementQuery portionRequestCase1 = new();
        portionRequestCase1.Presentation = SCPresentation.Open(Resources._020);
        portionRequestCase1.SlideIndex = 2;
        portionRequestCase1.ShapeId = 7;
        portionRequestCase1.ParagraphIndex = 0;
        portionRequestCase1.PortionIndex = 0;

        TestElementQuery portionRequestCase2 = new();
        portionRequestCase2.Presentation = SCPresentation.Open(Resources._026);
        portionRequestCase2.SlideIndex = 0;
        portionRequestCase2.ShapeId = 128;
        portionRequestCase2.ParagraphIndex = 0;
        portionRequestCase2.PortionIndex = 0;

        var testCases = new List<object[]>
        {
            new object[] { portionRequestCase1 },
            new object[] { portionRequestCase2 }
        };

        return testCases;
    }

    [Fact]
    public void IsItalic_GetterReturnsTrue_WhenFontOfNonPlaceholderTextIsItalic()
    {
        // Arrange
        IAutoShape nonPlaceholderAutoShape = (IAutoShape)_fixture.Pre020.Slides[0].Shapes.First(sp => sp.Id == 3);
        IFont font = nonPlaceholderAutoShape.TextFrame.Paragraphs[0].Portions[0].Font;

        // Act
        bool isItalicFont = font.IsItalic;

        // Assert
        isItalicFont.Should().BeTrue();
    }

    [Fact]
    public void IsItalic_GetterReturnsTrue_WhenFontOfPlaceholderTextIsItalic()
    {
        // Arrange
        IAutoShape placeholderAutoShape = (IAutoShape)_fixture.Pre020.Slides[2].Shapes.First(sp => sp.Id == 7);
        IPortion portion = placeholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act-Assert
        portion.Font.IsItalic.Should().BeTrue();
    }

    [Fact]
    public void IsItalic_Setter_SetsItalicFontForForNonPlaceholderText()
    {
        // Arrange
        var mStream = new MemoryStream();
        IPresentation presentation = SCPresentation.Open(Resources._020);
        IAutoShape nonPlaceholderAutoShape = (IAutoShape)presentation.Slides[0].Shapes.First(sp => sp.Id == 2);
        IPortion portion = nonPlaceholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act
        portion.Font.IsItalic = true;

        // Assert
        portion.Font.IsItalic.Should().BeTrue();
        presentation.SaveAs(mStream);
        presentation = SCPresentation.Open(mStream);
        nonPlaceholderAutoShape = (IAutoShape)presentation.Slides[0].Shapes.First(sp => sp.Id == 2);
        portion = nonPlaceholderAutoShape.TextFrame.Paragraphs[0].Portions[0];
        portion.Font.IsItalic.Should().BeTrue();
    }

    [Fact]
    public void IsItalic_SetterSetsNonItalicFontForPlaceholderText_WhenFalseValueIsPassed()
    {
        // Arrange
        var mStream = new MemoryStream();
        IPresentation presentation = SCPresentation.Open(Resources._020);
        IAutoShape placeholderAutoShape = (IAutoShape)presentation.Slides[2].Shapes.First(sp => sp.Id == 7);
        IPortion portion = placeholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act
        portion.Font.IsItalic = false;

        // Assert
        portion.Font.IsItalic.Should().BeFalse();
        presentation.SaveAs(mStream);

        presentation = SCPresentation.Open(mStream);
        placeholderAutoShape = (IAutoShape)presentation.Slides[2].Shapes.First(sp => sp.Id == 7);
        portion = placeholderAutoShape.TextFrame.Paragraphs[0].Portions[0];
        portion.Font.IsItalic.Should().BeFalse();
    }

    [Fact]
    public void Underline_SetUnderlineFont_WhenValueEqualsSetPassed()
    {
        // Arrange
        var mStream = new MemoryStream();
        IPresentation presentation = SCPresentation.Open(Resources._020);
        IAutoShape placeholderAutoShape = (IAutoShape)presentation.Slides[2].Shapes.First(sp => sp.Id == 7);
        IPortion portion = placeholderAutoShape.TextFrame.Paragraphs[0].Portions[0];

        // Act
        portion.Font.Underline = DocumentFormat.OpenXml.Drawing.TextUnderlineValues.Single;

        // Assert
        portion.Font.Underline.Should().Be(DocumentFormat.OpenXml.Drawing.TextUnderlineValues.Single);
        presentation.SaveAs(mStream);

        presentation = SCPresentation.Open(mStream);
        placeholderAutoShape = (IAutoShape)presentation.Slides[2].Shapes.First(sp => sp.Id == 7);
        portion = placeholderAutoShape.TextFrame.Paragraphs[0].Portions[0];
        portion.Font.Underline.Should().Be(DocumentFormat.OpenXml.Drawing.TextUnderlineValues.Single);
    }

    [Theory]
    [MemberData(nameof(TestCasesOffsetGetter))]
    public void OffsetEffect_Getter_returns_offset_of_Text(TestCase testCase)
    {
        // Arrange
        var font = testCase.AutoShape.TextFrame!.Paragraphs[0].Portions[1].Font;
        var expectedOffsetSize = testCase.ExpectedInt;

        // Act
        var offsetSize = font.OffsetEffect;

        // Assert
        offsetSize.Should().Be(expectedOffsetSize);
    }

    public static IEnumerable<object[]> TestCasesOffsetGetter
    {
        get
        {
            var testCase1 = new TestCase("#1");
            testCase1.PresentationName = "autoshape-case010.pptx";
            testCase1.SlideNumber = 1;
            testCase1.ShapeId = 2;
            testCase1.ExpectedInt = 50;
            yield return new object[] { testCase1 };

            var testCase2 = new TestCase("#2");
            testCase2.PresentationName = "autoshape-case010.pptx";
            testCase2.SlideNumber = 2;
            testCase2.ShapeName = "Title 1";
            testCase2.ExpectedInt = -32;
            yield return new object[] { testCase2 };
        }
    }

    [Theory]
    [MemberData(nameof(TestCasesOffsetSetter))]
    public void OffsetEffect_Setter_changes_Offset_of_paragraph_portion(TestCase testCase)
    {
        // Arrange
        var pres = testCase.Presentation;
        var font = testCase.AutoShape.TextFrame!.Paragraphs[0].Portions[0].Font;
        int superScriptOffset = testCase.ExpectedInt;
        var mStream = new MemoryStream();
        var oldOffsetSize = font.OffsetEffect;

        // Act
        font.OffsetEffect = superScriptOffset;
        pres.SaveAs(mStream);

        // Assert
        testCase.SetPresentation(mStream);
        font = testCase.AutoShape.TextFrame!.Paragraphs[0].Portions[0].Font;
        font.OffsetEffect.Should().NotBe(oldOffsetSize);
        font.OffsetEffect.Should().Be(superScriptOffset);
    }

    public static IEnumerable<object[]> TestCasesOffsetSetter
    {
        get
        {
            var testCase1 = new TestCase("#1");
            testCase1.PresentationName = "autoshape-case010.pptx";
            testCase1.SlideNumber = 3;
            testCase1.ShapeId = 2;
            testCase1.ExpectedInt = 12;
            yield return new object[] { testCase1 };

            var testCase2 = new TestCase("#2");
            testCase2.PresentationName = "autoshape-case010.pptx";
            testCase2.SlideNumber = 4;
            testCase2.ShapeName = "Title 1";
            testCase2.ExpectedInt = -27;
            yield return new object[] { testCase2 };
        }
    }
}