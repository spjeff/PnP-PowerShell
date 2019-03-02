using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using SharePointPnP.PowerShell.CmdletHelpAttributes;
using SharePointPnP.PowerShell.Commands.Base.PipeBinds;

namespace SharePointPnP.PowerShell.Commands.Lists
{
    //TODO: Create Test
    [Cmdlet(VerbsCommon.Get, "PnPListPermission")]
    [CmdletHelp("Get list permissions",
        Category = CmdletHelpCategory.Lists)]
    [CmdletExample(
        Code = "PS:> Get-PnPListPermission -Identity 'Documents' -User 'user@contoso.com' -AddRole 'Contribute'",
        Remarks = "This will return the permissions to the user 'user@contoso.com' for the list 'Documents'",
        SortOrder = 1)]        
    [CmdletExample(
        Code = "PS:> Get-PnPListPermission -Identity 'Documents' -User 'user@contoso.com' -RemoveRole 'Contribute'",
        Remarks = "This will return the permissions to the user 'user@contoso.com' for the list 'Documents'",
        SortOrder = 2)]        
    public class GetListPermission : PnPWebCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = ParameterAttribute.AllParameterSets, HelpMessage = "The ID or Title of the list.")]
        public ListPipeBind Identity;

        [Parameter(Mandatory = false, ParameterSetName = "Group")]
        public GroupPipeBind Group;

        [Parameter(Mandatory = false, ParameterSetName = "User")]
        public string User;

        protected override void ExecuteCmdlet()
        {
            var list = Identity.GetList(SelectedWeb);

            if (list != null)
            {
                Principal principal = null;
                if (ParameterSetName == "Group")
                {
                    if (Group.Id != -1)
                    {
                        principal = SelectedWeb.SiteGroups.GetById(Group.Id);
                    }
                    else if (!string.IsNullOrEmpty(Group.Name))
                    {
                        principal = SelectedWeb.SiteGroups.GetByName(Group.Name);
                    }
                    else if (Group.Group != null)
                    {
                        principal = Group.Group;
                    }
                }
                else
                {
                    principal = SelectedWeb.EnsureUser(User);
                    ClientContext.ExecuteQueryRetry();
                }
                if (principal != null)
                {
                    var roleAssignment = list.RoleAssignments.GetByPrincipal(principal);
                    var roleDefinitionBindings = roleAssignment.RoleDefinitionBindings;
                    foreach (var role in roleDefinitionBindings)
                    {
                        WriteObject(role, true);
                    }
                }
                else
                {
                    var roleAssignment = list.RoleAssignments;
                    var roleDefinitionBindings = roleAssignment.RoleDefinitionBindings;
                    foreach (var role in roleDefinitionBindings)
                    {
                        WriteObject(role, true);
                    }
                }
            }
        }
    }
}
