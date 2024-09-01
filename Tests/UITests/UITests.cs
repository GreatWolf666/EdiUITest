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
        protected override FlaUI.Core.AutomationBase GetAutomation()
        {
            var automation = new UIA3Automation();
            return automation;
          
        }
        
        protected override Application StartApplication()
        {
            var workingDirectory = Environment.CurrentDirectory;

            var application = Application.Launch(Path.Combine([Directory.GetParent(workingDirectory).Parent.Parent.Parent.Parent.FullName, Directory.GetParent(workingDirectory).Name, "Edi.exe"]));
            application.WaitWhileMainHandleIsMissing();
            System.Threading.Thread.Sleep(1000);
            return application;
        }
        protected static ConditionFactory GetConditionFactory()
        {
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            return cf;
        }

        [Test]
        public void VerifyApplicationLoaded()
        {
            Window window = Application.GetMainWindow(new UIA3Automation());

            Assert.That(window, Is.Not.Null);

        }

        [Test]
        public void VerifyNewDocumentButtonPresent()
        {
            Window window = Application.GetMainWindow(new UIA3Automation());
            ConditionFactory cf = new ConditionFactory(new UIA3PropertyLibrary());

            var NewDocument = window.FindFirstDescendant(cf.ByAutomationId("New")).AsButton();
            Assert.That(NewDocument, Is.Not.Null);
        }

        [Test]
        public void isNewDocumentClickable()
        {
            var window = Application.GetMainWindow(new UIA3Automation());
            var newDocument = window.FindFirstDescendant(GetConditionFactory().ByAutomationId("New")).AsButton();
            newDocument.Click();
            var untitledDocument = window.FindFirstDescendant(GetConditionFactory().ByLocalizedControlType("text"));
            Assert.That(untitledDocument, Is.Not.Null);
        }

        [Test]
        public void DocumentClose()
        {
            var window = Application.GetMainWindow(new UIA3Automation());
            window.FindFirstDescendant(GetConditionFactory().ByAutomationId("DocumentCloseButton")).AsButton().Click();
            var closedDocument = window.FindFirstByXPath("/Custom[3]/Tab/TabItem[2]").AsTabItem();
            Assert.That(closedDocument, Is.Null);
        }
        
        [Test]
        public void IsExistingDocumentOpenable()
        {
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
            var openWindow = window.FindFirstChild(GetConditionFactory().ByClassName("#32770"));
            var fileInput = openWindow.FindFirstDescendant(GetConditionFactory().ByAutomationId("1148")).AsComboBox();
            fileInput.Focus();

            fileInput.EditableText = fileFullPath;

            openWindow.FindFirstChild(GetConditionFactory().ByAutomationId("1")).AsButton().Click();

            var tabItem = window.FindFirstChild(GetConditionFactory().ByAutomationId("dockView")).FindFirstDescendant(GetConditionFactory().ByHelpText(fileFullPath));
            var textView = tabItem.FindFirstChild(GetConditionFactory().ByClassName("EdiView")).FindFirstChild();
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

        [Test]

        public void IsDocumentEditable()
        {

            var window = Application.GetMainWindow(new UIA3Automation());
            var newDocument = window.FindFirstDescendant(GetConditionFactory().ByAutomationId("New")).AsButton();
            newDocument.Click();
            var untitledDocument = window.FindFirstDescendant(GetConditionFactory().ByLocalizedControlType("text"));
            var tabItem = window.FindFirstChild(GetConditionFactory().ByAutomationId("dockView")).FindFirstDescendant(GetConditionFactory().ByHelpText("Untitled.txt"));
            var textView = tabItem.FindFirstChild(GetConditionFactory().ByClassName("EdiView")).FindFirstChild();

            if (textView.Patterns.Value.TryGetPattern(out var inputText))
            {
                ((ValuePattern)inputText).SetValue("whatever");
                Assert.That(true, Is.True);
            }
            else
            {
                Assert.Fail();
            }

            var editedDocument = tabItem.FindFirstChild(GetConditionFactory().ByClassName("TextBlock"));

            StringAssert.EndsWith(editedDocument.Name, "*");

           

            Application.Close(true);

        }
    }
}