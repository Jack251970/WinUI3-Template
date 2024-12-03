using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;

namespace WinUI3Template.TemplateWizard
{
    public class PageViewViewModelTemplateWizard : IWizard
    {
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            // Get the root namespace and safe item name
            var rootNamespace = replacementsDictionary["$rootnamespace$"];
            var safeItemName = replacementsDictionary["$safeitemname$"];

            // Modify $safeitemname$ to create a new value
            var viewModelNamespace = rootNamespace.Replace("Views", "ViewModels");
            var viewModelName = safeItemName.Replace("Page", "ViewModel");
            var behaviorNamespace = rootNamespace.Replace("Views.Pages", "Behaviors");

            // Add the custom parameter to the replacements dictionary
            replacementsDictionary["$viewmodelnamespace$"] = viewModelNamespace;
            replacementsDictionary["$viewmodelname$"] = viewModelName;
            replacementsDictionary["$behaviornamespace$"] = behaviorNamespace;
        }

        public void RunFinished() { }

        public void BeforeOpeningFile(global::EnvDTE.ProjectItem projectItem) { }

        public void ProjectFinishedGenerating(global::EnvDTE.Project project) { }

        public void ProjectItemFinishedGenerating(global::EnvDTE.ProjectItem projectItem) { }

        public bool ShouldAddProjectItem(string filePath) => true;
    }
}
