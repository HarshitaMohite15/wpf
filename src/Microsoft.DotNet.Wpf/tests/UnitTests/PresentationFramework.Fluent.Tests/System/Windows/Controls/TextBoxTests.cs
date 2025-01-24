// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Controls
{
    public class TextBoxTests
    {
        #region Creation Tests

        [WpfFact]
        public void TextBox_ShouldBeCreated()
        {
            // Act
            var textBox = new TextBox();

            // Assert
            Assert.NotNull(textBox);
        }

        #endregion

        #region Text Property Tests

        [WpfFact]
        public void TextBox_DefaultText_ShouldBeEmpty()
        {
            // Arrange
            var textBox = new TextBox();

            // Act
            var text = textBox.Text;

            // Assert
            Assert.Equal(string.Empty, text);
        }

        [WpfFact]
        public void TextBox_SetText_ShouldUpdateTextProperty()
        {
            // Arrange
            var textBox = new TextBox();
            var expectedText = "Hello, World!";

            // Act
            textBox.Text = expectedText;
            var actualText = textBox.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        [WpfFact]
        public void TextBox_EnterAlphaNumericText_ShouldUpdateTextProperty()
        {
            // Arrange
            var textBox = new TextBox();
            var inputText = "/\\d.*[a-zA-Z]|[a-zA-Z].*\\d/";

            // Act
            textBox.Text = inputText;
            var actualText = textBox.Text;

            // Assert
            Assert.Equal(inputText, actualText);
        }

        [WpfFact]
        public void TextBox_EnterSpecialCharText_ShouldUpdateTextProperty()
        {
            // Arrange
            var textBox = new TextBox();
            var expectedText = "@#$%^&*";

            // Act
            textBox.Text = expectedText;
            var actualText = textBox.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        [WpfFact]
        public void TextBox_EnterEmailIDText_ShouldUpdateTextProperty()
        {
            // Arrange
            var textBox = new TextBox();
            var expectedText = "ram.Shay1234@yahoo.com";

            // Act
            textBox.Text = expectedText;
            var actualText = textBox.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        [WpfFact]
        public void TextBox_EnterHtmlText_ShouldUpdateTextProperty()
        {
            // Arrange
            var textBox = new TextBox();
            var expectedText = "<script>alert(\"123\")</script>";

            // Act
            textBox.Text = expectedText;
            var actualText = textBox.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        [WpfFact]
        public void TextBox_ClearText_ShouldSetTextToEmpty()
        {
            // Arrange
            var textBox = new TextBox
            {
                Text = "Some Text"
            };

            // Act
            textBox.Clear();
            var text = textBox.Text;

            // Assert
            Assert.Equal(string.Empty, text);
        }

        #endregion

        #region Multi-Line Tests

        [WpfFact]
        public void TextBox_MultiLineTextBox_ShouldAcceptMultipleLines()
        {
            // Arrange
            var textBox = new TextBox { AcceptsReturn = true };
            var multiLineText = "Line1\nLine2\nLine3";

            // Act
            textBox.Text = multiLineText;
            var actualText = textBox.Text;

            // Assert
            Assert.Equal(multiLineText, actualText);
        }

        #endregion

        #region Event Tests

        [WpfFact]
        public void TextBox_TextChangedEvent_ShouldBeRaised()
        {
            // Arrange
            var textBox = new TextBox();
            var eventRaised = false;
            textBox.TextChanged += (sender, args) => eventRaised = true;

            // Act
            textBox.Text = "New Text";

            // Assert
            Assert.True(eventRaised);
        }

        #endregion

        [WpfFact]
        public void TextBox_SelectText_ShouldSelectCorrectText()
        {
            // Arrange
            var textBox = new TextBox();
            var inputText = "Hello, World!";
            textBox.Text = inputText;

            // Act
            textBox.Select(7, 5); // Select "World"
            var selectedText = textBox.SelectedText;

            // Assert
            Assert.Equal("World", selectedText);
        }

        #region Clipboard Operations Tests

        [WpfFact]
        public void TextBox_CutText_ShouldRemoveSelectedTextAndCopyToClipboard()
        {
            // Arrange
            var textBox = new TextBox { Text = "Hello, World!" };
            textBox.Select(7, 5); // Select "World"

            // Act
            textBox.Cut();
            var textAfterCut = textBox.Text;
            var clipboardText = Clipboard.GetText();

            // Assert
            Assert.Equal("Hello, !", textAfterCut);
            Assert.Equal("World", clipboardText);
        }

        [WpfFact]
        public void TextBox_CopyText_ShouldCopySelectedTextToClipboard()
        {
            // Arrange
            var textBox = new TextBox { Text = "Hello, World!" };
            textBox.Select(7, 5); // Select "World"

            // Act
            textBox.Copy();
            var clipboardText = Clipboard.GetText();

            // Assert
            Assert.Equal("World", clipboardText);
        }

        [WpfFact]
        public void TextBox_PasteText_ShouldInsertClipboardTextAtCaretPosition()
        {
            // Arrange
            var textBox = new TextBox
            {
                Text = "Hello, !",
                CaretIndex = 7 // Set caret position after "Hello, "
            };
            Clipboard.SetText("World");

            // Act
            textBox.Paste();
            var textAfterPaste = textBox.Text;

            // Assert
            Assert.Equal("Hello, World!", textAfterPaste);
        }

        [WpfFact]
        public void TextBox_Undo_ShouldRevertLastChange_OnRightClick()
        {
            // Arrange
            var textBox = new TextBox { Text = "Hello, World!" };
            textBox.Text = "New Text";

            //// Simulate right-click and select "Undo" (programmatically)
            //var contextMenu = new ContextMenu();
            //var undoMenuItem = new MenuItem { Header = "Undo" };
            //undoMenuItem.Click += (sender, args) => textBox.Undo();
            //contextMenu.Items.Add(undoMenuItem);
            //textBox.ContextMenu = contextMenu;

            // Act
            textBox.Undo();
            textBox.Undo();
            var textAfterUndo = textBox.Text;

            // Assert
            Assert.Equal("Hello, World!", textAfterUndo);
        }
        [WpfFact]
        public void TextBox_Redo_ShouldReapplyLastUndoneChange()
        {
            // Arrange
            var textBox = new TextBox { Text = "Hello, World!" };
            textBox.Text = "New Text";
            textBox.Undo();

            // Act
            textBox.Redo();
            var textAfterRedo = textBox.Text;

            // Assert
            Assert.Equal("New Text", textAfterRedo);
        }

        #endregion
    }
}
