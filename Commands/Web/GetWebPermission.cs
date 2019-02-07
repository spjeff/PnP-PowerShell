using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using SharePointPnP.PowerShell.CmdletHelpAttributes;
using SharePointPnP.PowerShell.Commands.Base.PipeBinds;
using SharePointPnP.PowerShell.Commands.Extensions;

namespace SharePointPnP.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "PnPWebPermission", DefaultParameterSetName = "User")]
    [CmdletHelp("Get permissions",
        "Get web permissions",
        Category = CmdletHelpCategory.Webs)]
    [CmdletExample(
        Code = "PS:> Get-PnPWebPermission -Url projectA",
        Remarks = "This will return the permissions for a web, specified by its site relative url",
        SortOrder = 1)] 
    [CmdletExample(
        Code = "PS:> Get-PnPWebPermission -Url projectA -User 'user@contoso.com'",
        Remarks = "This will return the permissions for the user 'user@contoso.com' for a web, specified by its site relative url",
        SortOrder = 2)]        
    [CmdletExample(
        Code = "PS:> Get-PnPWebPermission -Identity 5fecaf67-6b9e-4691-a0ff-518fc9839aa0 -User 'user@contoso.com'",
        Remarks = "This will return the permissions for the user 'user@contoso.com' for a web, specified by its ID",
        SortOrder = 3)]        
    public class GetWebPermission : PnPWebCmdlet
    {
		[Parameter(Mandatory = true, HelpMessage = "Identity/Id/Web object", ParameterSetName = "GroupByWebIdentity", ValueFromPipeline = true)]
		[Parameter(Mandatory = true, HelpMessage = "Identity/Id/Web object", ParameterSetName = "UserByWebIdentity", ValueFromPipeline = true)]
		public WebPipeBind Identity;

		[Parameter(Mandatory = true, HelpMessage = "The site relative url of the web, e.g. 'Subweb1'", ParameterSetName = "GroupByWebUrl")]
		[Parameter(Mandatory = true, HelpMessage = "The site relative url of the web, e.g. 'Subweb1'", ParameterSetName = "UserByWebUrl")]
		public string Url;

		[Parameter(Mandatory = false, ParameterSetName = "Group")]
		[Parameter(Mandatory = false, ParameterSetName = "GroupByWebIdentity")]
		[Parameter(Mandatory = false, ParameterSetName = "GroupByWebUrl")]
		public GroupPipeBind Group;

        [Parameter(Mandatory = false, ParameterSetName = "User")]
        [Parameter(Mandatory = false, ParameterSetName = "UserByWebIdentity")]
        [Parameter(Mandatory = false, ParameterSetName = "UserByWebUrl")]
        public string User;


        protected override void ExecuteCmdlet()
		{
			// Get Web
			Web web = SelectedWeb;
			if (ParameterSetName == "GroupByWebIdentity" || ParameterSetName == "UserByWebIdentity")
			{
				if (Identity.Id != Guid.Empty)
				{
					web = ClientContext.Web.GetWebById(Identity.Id);
				}
				else if (Identity.Web != null)
				{
					web = Identity.Web;
				}
				else if (Identity.Url != null)
				{
					web = ClientContext.Web.GetWebByUrl(Identity.Url);
				}
			}
			else if (ParameterSetName == "GroupByWebUrl" || ParameterSetName == "UserByWebUrl")
			{
				web = SelectedWeb.GetWeb(Url);
			}

			// Get permissions
			Principal principal = null;
			if (ParameterSetName == "Group" || ParameterSetName == "GroupByWebUrl" || ParameterSetName == "GroupByWebIdentity")
			{
				if (Group.Id != -1)
				{
					principal = web.SiteGroups.GetById(Group.Id);
				}
				else if (!string.IsNullOrEmpty(Group.Name))
				{
					principal = web.SiteGroups.GetByName(Group.Name);
				}
				else if (Group.Group != null)
				{
					principal = Group.Group;
				}
			}
			else
			{
				principal = web.EnsureUser(User);
			}

			// Principal
			if (principal != null)
			{
				foreach (var role in web.RoleAssigments)
				{
					// Filter to Principal
					if (role.Member == principal) {
						WriteObject(role, true);
					}
				}
			}
			else
			{
				foreach (var role in web.RoleAssigments)
				{
					// Filter to Principal
					WriteObject(role, true);
					
				}
			}
		}
	}
}
