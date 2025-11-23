// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Окно поиска и замены
    /// </summary>
    public partial class WinFindReplaceDialog : Window
    {
        private static string textToFind = "";

        /// <summary>
        /// =true - с учетом регистра
        /// </summary>
        public static bool caseSensitive = true;

        /// <summary>
        /// =true - слово целиком
        /// </summary>
        public static bool wholeWord = true;

        /// <summary>
        /// =true - использовать регулярное выражение
        /// </summary>
        public static bool useRegex = false;

        /// <summary>
        /// =true - использовать wildcards
        /// </summary>
        public static bool useWildcards = false;

        /// <summary>
        /// =true - искать в обратную сторону
        /// </summary>
        public static bool searchUp = false;

        private TextEditor editor;

        /// <summary>
        /// Конструктор WinFindReplaceDialog
        /// </summary>
        /// <param name="editor">ссылка на редактор TextEditor</param>
        public WinFindReplaceDialog(TextEditor editor)
        {
            InitializeComponent();

            this.editor = editor;

            txtFind.Text = txtFind2.Text = textToFind;
            cbCaseSensitive.IsChecked = caseSensitive;
            cbWholeWord.IsChecked = wholeWord;
            cbRegex.IsChecked = useRegex;
            cbWildcards.IsChecked = useWildcards;
            cbSearchUp.IsChecked = searchUp;
        }

        /// <summary>
        /// Окно закрывается
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            textToFind = txtFind2.Text;
            caseSensitive = (cbCaseSensitive.IsChecked == true);
            wholeWord = (cbWholeWord.IsChecked == true);
            useRegex = (cbRegex.IsChecked == true);
            useWildcards = (cbWildcards.IsChecked == true);
            searchUp = (cbSearchUp.IsChecked == true);

            theDialog = null;
        }

        private void FindNextClick(object sender, RoutedEventArgs e)
        {
            if (!FindNext(txtFind.Text))
                SystemSounds.Beep.Play();
        }

        private void FindNext2Click(object sender, RoutedEventArgs e)
        {
            if (!FindNext(txtFind2.Text))
                SystemSounds.Beep.Play();
        }

        private void ReplaceClick(object sender, RoutedEventArgs e)
        {
            Regex regex = GetRegEx(txtFind2.Text);
            string input = editor.Text.Substring(editor.SelectionStart, editor.SelectionLength);
            Match match = regex.Match(input);
            bool replaced = false;
            if (match.Success && match.Index == 0 && match.Length == input.Length)
            {
                editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, txtReplace.Text);
                replaced = true;
            }

            if (!FindNext(txtFind2.Text) && !replaced)
                SystemSounds.Beep.Play();
        }

        private void ReplaceAllClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Replace All occurences of \"" +
            txtFind2.Text + "\" with \"" + txtReplace.Text + "\"?",
                "Replace All", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Regex regex = GetRegEx(txtFind2.Text, true);
                int offset = 0;
                editor.BeginChange();
                foreach (Match match in regex.Matches(editor.Text))
                {
                    editor.Document.Replace(offset + match.Index, match.Length, txtReplace.Text);
                    offset += txtReplace.Text.Length - match.Length;
                }
                editor.EndChange();
            }
        }

        private bool FindNext(string textToFind)
        {
            Regex regex = GetRegEx(textToFind);
            int start = regex.Options.HasFlag(RegexOptions.RightToLeft) ?
            editor.SelectionStart : editor.SelectionStart + editor.SelectionLength;
            Match match = regex.Match(editor.Text, start);

            if (!match.Success)  // start again from beginning or end
            {
                if (regex.Options.HasFlag(RegexOptions.RightToLeft))
                    match = regex.Match(editor.Text, editor.Text.Length);
                else
                    match = regex.Match(editor.Text, 0);
            }

            if (match.Success)
            {
                editor.Select(match.Index, match.Length);
                TextLocation loc = editor.Document.GetLocation(match.Index);
                editor.ScrollTo(loc.Line, loc.Column);
            }

            return match.Success;
        }

        private Regex GetRegEx(string textToFind, bool leftToRight = false)
        {
            RegexOptions options = RegexOptions.None;
            if (cbSearchUp.IsChecked == true && !leftToRight)
                options |= RegexOptions.RightToLeft;
            if (cbCaseSensitive.IsChecked == false)
                options |= RegexOptions.IgnoreCase;

            if (cbRegex.IsChecked == true)
            {
                return new Regex(textToFind, options);
            }
            else
            {
                string pattern = Regex.Escape(textToFind);
                if (cbWildcards.IsChecked == true)
                    pattern = pattern.Replace("\\*", ".*").Replace("\\?", ".");
                if (cbWholeWord.IsChecked == true)
                    pattern = "\\b" + pattern + "\\b";
                return new Regex(pattern, options);
            }
        }

        private static WinFindReplaceDialog theDialog = null;

        /// <summary>
        /// Открыть окно поиска и замены
        /// </summary>
        /// <param name="editor">редактор TextEditor</param>
        /// <param name="isReplace">=true - замена</param>
        public static void ShowForReplace(TextEditor editor, bool isReplace)
        {
            if (theDialog == null)
            {
                theDialog = new WinFindReplaceDialog(editor);
                if (isReplace == true) theDialog.tabMain.SelectedIndex = 1;
                else theDialog.tabMain.SelectedIndex = 0;
                theDialog.Show();
                theDialog.Activate();
            }
            else
            {
                if (isReplace == true) theDialog.tabMain.SelectedIndex = 1;
                else theDialog.tabMain.SelectedIndex = 0;
                theDialog.Activate();
            }

            if (!editor.TextArea.Selection.IsMultiline)
            {
                theDialog.txtFind.Text = theDialog.txtFind2.Text = editor.TextArea.Selection.GetText();
                theDialog.txtFind.SelectAll();
                theDialog.txtFind2.SelectAll();
                theDialog.txtFind2.Focus();
            }
        }
    }
}
