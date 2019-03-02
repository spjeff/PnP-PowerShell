using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using SharePointPnP.PowerShell.CmdletHelpAttributes;
using SharePointPnP.PowerShell.Commands.Base.PipeBinds;

namespace SharePointPnP.PowerShell.Commands.Lists
{
    [Cmdlet(VerbsCommon.Get, "PnPListItemPermission", DefaultParameterSetName = "User")]
    [CmdletHelp("Get list item permissions",
        Category = CmdletHelpCategory.Lists)]
    [CmdletExample(
        Code = "PS:> Get-PnPListItemPermission -List 'Documents' -Identity 1 -User 'user@contoso.com'",
        Remarks = "This will return the permissions for the user 'user@contoso.com' and listitem with id 1 in the list 'Documents'",
        SortOrder = 1)]
    [CmdletExample(
        Code = "PS:> Get-PnPListItemPermission -List 'Documents' -Identity 1",
        Remarks = "This will return the permissions for listitem with id 1 in the list 'Documents'",
        SortOrder = 2)]
    public class GetListItemPermission : PnPWebCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, HelpMessage = "The ID, Title or Url of the list.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public ListPipeBind List;

        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The ID of the listitem, or actual ListItem object", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public ListItemPipeBind Identity;

        [Parameter(Mandatory = true, ParameterSetName = "Group")]
        public GroupPipeBind Group;

        [Parameter(Mandatory = true, ParameterSetName = "User")]
        public string User;

        protected override void ExecuteCmdlet()
        {
            List list = null;
            if (List != null)
            {
                list = List.GetList(SelectedWeb);
            }
            if (list != null)
            {
                var item = Identity.GetListItem(list);
                if (item != null)
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
                        var roleAssignment = item.RoleAssignments.GetByPrincipal(principal);
                        var roleDefinitionBindings = roleAssignment.RoleDefinitionBindings;
                        foreach (var role in roleDefinitionBindings)
                        {
                            WriteObject(role, true);
                        }
                    }
                    else
                    {
                        var roleAssignment = item.RoleAssignments;
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
}