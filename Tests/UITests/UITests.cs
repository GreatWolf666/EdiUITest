using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.TestUtilities;
using FlaUI.UIA3;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using System.Reflection;
using ValuePattern = FlaUI.UIA3.Patterns.ValuePattern;
using FlaUI.Core.Tools;
using System.Security.Cryptography.X509Certificates;

namespace UITests

{
    [TestFixture]
    public class UITests : FlaUITestBase
    {
        // This method sets up the automation for FlaUI
        protected override FlaUI.Core.AutomationBase GetAutomation()
        {
            var automation = new UIA3Automation();
            return automation;
          
        }
        
        // This method sets up the application start
        protected override Application StartApplication()
        {
            // This variable is set up so the test cases are runnable by anyone who builds the main project
            var workingDirectory = Environment.CurrentDirectory;

            // This is the pathing variable, it ensures the automated tests find the runnable executable
            var application = Application.Launch(Path.Combine([Directory.GetParent(workingDirectory).Parent.Parent.Parent.Parent.FullName, Directory.GetParent(workingDirectory).Name, "Edi.exe"]));
            
            // This makes sure to wait for the application to start before the executing the testcases
            application.WaitWhileMainHandleIsMissing();
            System.Threading.Thread.Sleep(1000);
            return application;
        }
        protected static ConditionFactory GetConditionFactory()
        {
            // Setting up the condition factory, which enables us to use the find functions in FlaUI
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            return cf;
        }

        // This testcase verifies, that the application starts, expected result is that the main window is not null
        [Test]
        public void VerifyApplicationLoaded()
        {
            
            Window window = Application.GetMainWindow(new UIA3Automation());

            Assert.That(window, Is.Not.Null);

        }

        // This test case verifies, that the application has the new document button, expected result is the UI element as a button is not null
        [Test]
        public void VerifyNewDocumentButtonPresent()
        {
            Window window = Application.GetMainWindow(new UIA3Automation());
            ConditionFactory cf = new ConditionFactory(new UIA3PropertyLibrary());

            var NewDocument = window.FindFirstDescendant(cf.ByAutomationId("New")).AsButton();
            Assert.That(NewDocument, Is.Not.Null);
        }

        // This test case verifies, that the new document button is present and clickable, expected result is that after clicking on "New" opens a new tab with a clean text document
        [Test]
        public void isNewDocumentClickable()
        {
            var window = Application.GetMainWindow(new UIA3Automation());
            var newDocument = window.FindFirstDescendant(GetConditionFactory().ByAutomationId("New")).AsButton();
            newDocument.Click();
            var untitledDocument = window.FindFirstDescendant(GetConditionFactory().ByLocalizedControlType("text"));
            Assert.That(untitledDocument, Is.Not.Null);
        }

        // This testcase verifies, that the document can be closed by the tab item "x" button, expected result is that the document has closed, thus the UI element is null
        [Test]
        public void DocumentClose()
        {
            var window = Application.GetMainWindow(new UIA3Automation());
            window.FindFirstDescendant(GetConditionFactory().ByAutomationId("DocumentCloseButton")).AsButton().Click();
            var closedDocument = window.FindFirstByXPath("/Custom[3]/Tab/TabItem[2]").AsTabItem();
            Assert.That(closedDocument, Is.Null);
        }
        
        // This test case verifies that an already existing text document can be opened (this is included in this project, opened through dynamic pathing), expected result is that, that the document is opened, checked by document name
        // The fail condition is wrapped in an if statement, if it does not find the expected file title in the UI as opened it fails
        [Test]
        public void IsExistingDocumentOpenable()
        {
            // Here we set up the dynamic pathing variable
            var workingDirectory = Environment.CurrentDirectory;
            var fileFullPath = Path.Combine([Directory.GetParent(workingDirectory).Parent.Parent.Parent.Parent.FullName, "Tests\\UITests\\test.txt"]);
            
            var window = Application.GetMainWindow(new UIA3Automation());
            
            var fileMenu = window.FindFirstChild(GetConditionFactory().ByClassName("MainMenu")).FindFirstDescendant(GetConditionFactory().ByName("File"));
            fileMenu.AsMenuItem().Click();
            System.Threading.Thread.Sleep(1000);
            
            var newMenuItem = fileMenu.FindFirstDescendant(GetConditionFactory().ByName("Open"));
            newMenuItem.AsMenuItem().Click();
            var textDocumentMenuItem = newMenuItem.FindFirstDescendant(GetConditionFactory().ByName("Text files")).AsMenuItem();
            
            textDocumentMenuItem.WaitUntilClickable();
            textDocumentMenuItem.Click();
            System.Threading.Thread.Sleep(1000);
            
            // Here we are using the dynamic path variable in the new window to open the test file
            var openWindow = window.FindFirstChild(GetConditionFactory().ByClassName("#32770"));
            var fileInput = openWindow.FindFirstDescendant(GetConditionFactory().ByAutomationId("1148")).AsComboBox();
            
            // Here we put the UI element into focus, where we can make an input for the file path
            fileInput.Focus();

            fileInput.EditableText = fileFullPath;

            // This clicks on the Open button
            openWindow.FindFirstChild(GetConditionFactory().ByAutomationId("1")).AsButton().Click();

            // In this variable, we find the opened file in Edi
            var tabItem = window.FindFirstChild(GetConditionFactory().ByAutomationId("dockView")).FindFirstDescendant(GetConditionFactory().ByHelpText(fileFullPath));
            
            // In this variable is finding the opened file in the text editor element
            var textView = tabItem.FindFirstChild(GetConditionFactory().ByClassName("EdiView")).FindFirstChild();
            
            // Here we search for the test file title for our assertion, unfortunately it doesn't work without an if statement in FlaUI
            if (textView.Patterns.Value.TryGetPattern(out var valuePattern))
            {
                var textValue = ((ValuePattern)valuePattern).Value;
                Assert.That(textValue, Is.EqualTo("testfile"));
            }
            else
            {
                Assert.Fail();
            }

            
           
        }

        // This test case verifies, that a new text document opened is editable, expected result is that the new document is edited, checked by the "*" character in the opened documents UI tab
        // This test has multiple fail/pass points, first is editing the document, which is an internal assertion, as Edi is very old and uses a lot of custom/legacy UI elements that has to be worked around.
        // The input assertion's failiure does not mean the main test case fails, it is just a legacy workaround. It only indicates that we didn't find the UI element or could not input our string variable into the document.
        [Test]
        public void IsDocumentEditable()
        {

            var window = Application.GetMainWindow(new UIA3Automation());
            
            // Here we open a new fresh document
            var newDocument = window.FindFirstDescendant(GetConditionFactory().ByAutomationId("New")).AsButton();
            
            newDocument.Click();
            
            var untitledDocument = window.FindFirstDescendant(GetConditionFactory().ByLocalizedControlType("text"));
            var tabItem = window.FindFirstChild(GetConditionFactory().ByAutomationId("dockView")).FindFirstDescendant(GetConditionFactory().ByHelpText("Untitled.txt"));
            var textView = tabItem.FindFirstChild(GetConditionFactory().ByClassName("EdiView")).FindFirstChild();

            // Here we feed an input string to the newly opened document, it has to be wrapped in an if statement
            if (textView.Patterns.Value.TryGetPattern(out var inputText))
            {
                ((ValuePattern)inputText).SetValue("whatever");
                Assert.That(true, Is.True);
            }
            else
            {
                Assert.Fail();
            }

            // Here we check if the "*" character, indicating the file has been edited but not saved is present at the end of the opened documents tab item
            var editedDocument = tabItem.FindFirstChild(GetConditionFactory().ByClassName("TextBlock"));

            StringAssert.EndsWith(editedDocument.Name, "*");

           // Here we close the application, it is a forced close, as checking if the document can be saved would be another test case, this way the test does not fail even if the main test assertion has passed
           // By passing the "true" boolean value to the Close action, we are giving a short timeout to the action before forcefully closing it, it doesn't indicate a failure
            Application.Close(true);

        }
    }
}