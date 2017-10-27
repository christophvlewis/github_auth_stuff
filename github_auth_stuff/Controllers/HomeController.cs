using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Octokit;
using OctokitDemo.Models;
using Microsoft.AspNetCore.Http;
using github_auth_stuff.Controllers;

namespace OctokitDemo.Controllers
{
	public class HomeController : Controller
	{
		// TODO: Replace the following values with the values from your application registration. Register an
		// application at https://github.com/settings/applications/new to get these values.
		const string clientId = "4f364f3d5d5227345104";
		private const string clientSecret = "3b9d3c86ee4b9934ee4e3d5a3a5fd17a64c05063";
		readonly GitHubClient client =
			new GitHubClient(new ProductHeaderValue("Haack-GitHub-Oauth-Demo"));

		private const string SessionKeyToken = "_Token";
		private const string SessionKeyCSRF = "CSRF:State";



		// This URL uses the GitHub API to get a list of the current user's
		// repositories which include public and private repositories.
		public async Task<ActionResult> Index()
		{
			var accessToken = HttpContext.Session.GetString(SessionKeyToken);
			if (accessToken != null)
			{
				// This allows the client to make requests to the GitHub API on the user's behalf
				// without ever having the user's OAuth credentials.
				client.Credentials = new Credentials(accessToken);
			}

			try
			{
				// The following requests retrieves all of the user's repositories and
				// requires that the user be logged in to work.


				// for looking at repositories of user
				var repositories = await client.Repository.GetAllForCurrent();
				//var model = new IndexViewModel(repositories);

				// for looking at users credentials
				var model = new IndexCredViewModel(client.Credentials);

				return View(model);
			}
			catch (AuthorizationException)
			{
				// Either the accessToken is null or it's invalid. This redirects
				// to the GitHub OAuth login page. That page will redirect back to the
				// Authorize action.
				return Redirect(GetOauthLoginUrl());
			}
		}

		// This is the Callback URL that the GitHub OAuth Login page will redirect back to.
		public async Task<ActionResult> Authorize(string code, string state)
		{
			if (!String.IsNullOrEmpty(code))
			{
				var expectedState = HttpContext.Session.GetString(SessionKeyCSRF);
				if (state != expectedState) throw new InvalidOperationException("SECURITY FAIL!");
				HttpContext.Session.SetString(SessionKeyCSRF, "");

				var token = await client.Oauth.CreateAccessToken(
					new OauthTokenRequest(clientId, clientSecret, code));
				HttpContext.Session.SetString(SessionKeyToken, token.AccessToken);
			}

			return RedirectToAction("Index");
		}

		private string GetOauthLoginUrl()
		{
			string csrf = Password.Generate(24, 1);
			HttpContext.Session.SetString(SessionKeyCSRF, csrf);

			// 1. Redirect users to request GitHub access
			var request = new OauthLoginRequest(clientId)
			{
				Scopes = { "user", "notifications" },
				State = csrf
			};
			var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);
			return oauthLoginUrl.ToString();
		}

	}
}