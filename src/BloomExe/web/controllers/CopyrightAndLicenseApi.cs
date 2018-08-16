using System;
using System.Drawing;
using Bloom.Api;
using Bloom.Edit;
using SIL.IO;
using SIL.Windows.Forms.ClearShare;

namespace Bloom.web.controllers
{
	public class CopyrightAndLicenseApi
	{
		public EditingModel Model { get; set; }

		public void RegisterWithApiHandler(BloomApiHandler apiHandler)
		{
			apiHandler.RegisterEndpointHandler("copyrightAndLicense/ccImage", HandleGetCCImage, false);
			apiHandler.RegisterEndpointHandler("copyrightAndLicense/getBookLicenseMetadata", HandleGetBookLicenseMetadata, true);
			apiHandler.RegisterEndpointHandler("copyrightAndLicense/saveBookLicenseMetadata", HandleSaveBookLicenseMetadata, true);
		}

		private void HandleGetCCImage(ApiRequest request)
		{
			lock (request)
			{
				try
				{
					Image licenseImage;
					var token = request.Parameters.GetValues("token");
					if (token != null)
						licenseImage = CreativeCommonsLicense.FromToken(token[0]).GetImage();
					else
						licenseImage = request.CurrentBook.GetLicenseMetadata().License.GetImage();
					using (TempFile tempFile = new TempFile())
					{
						licenseImage.Save(tempFile.Path);
						request.ReplyWithImage(tempFile.Path);
					}
				}
				catch
				{
					request.Failed();
				}
			}
		}

		private void HandleGetBookLicenseMetadata(ApiRequest request)
		{
			//in case we were in this dialog already and made changes which haven't found their way out to the book yet
			Model.SaveNow();

			var metadata = Model.CurrentBook.GetLicenseMetadata();
			dynamic creativeCommonsInfoJson = GetDefaultCreativeCommonsInfo();
			if (metadata.License is CreativeCommonsLicense)
				creativeCommonsInfoJson = GetCreativeCommonsInfo((CreativeCommonsLicense)metadata.License);

			var intellectualPropertyData = new
			{
				creator = metadata.Creator ?? string.Empty,
				copyrightYear = metadata.GetCopyrightYear() ?? string.Empty,
				copyrightHolder = metadata.GetCopyrightBy() ?? string.Empty,
				licenseInfo = new
				{
					licenseType = GetLicenseType(metadata.License),
					creativeCommonsInfo = creativeCommonsInfoJson,
					rightsStatement = metadata.License.RightsStatement ?? string.Empty
				},
			};

			request.ReplyWithJson(intellectualPropertyData);
		}

		private dynamic GetDefaultCreativeCommonsInfo()
		{
			return new
			{
				allowCommercial = "yes",
				allowDerivatives = "yes",
				intergovernmentalVersion = true
			};
		}

		private dynamic GetCreativeCommonsInfo(CreativeCommonsLicense ccLicense)
		{
			return new
			{
				allowCommercial = ccLicense.CommercialUseAllowed ? "yes" : "no",
				allowDerivatives = GetDerivativeRulesAsString(ccLicense.DerivativeRule),
				intergovernmentalVersion = ccLicense.IntergovernmentalOriganizationQualifier
			};
		}

		private static string GetDerivativeRulesAsString(CreativeCommonsLicense.DerivativeRules rules)
		{
			switch (rules)
			{
				case CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike:
					return "sharealike";
				case CreativeCommonsLicense.DerivativeRules.Derivatives:
					return "yes";
				case CreativeCommonsLicense.DerivativeRules.NoDerivatives:
					return "no";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private CreativeCommonsLicense.DerivativeRules GetDerivativeRule(string jsonValue)
		{
			switch (jsonValue)
			{
				case "sharealike":
					return CreativeCommonsLicense.DerivativeRules.DerivativesWithShareAndShareAlike;
				case "yes":
					return CreativeCommonsLicense.DerivativeRules.Derivatives;
				case "no":
					return CreativeCommonsLicense.DerivativeRules.NoDerivatives;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private string GetLicenseType(LicenseInfo licenseInfo)
		{
			if (licenseInfo is CreativeCommonsLicense)
				return "creativeCommons";
			if (licenseInfo is CustomLicense)
				return "custom";
			return "contact";
		}

		private void HandleSaveBookLicenseMetadata(ApiRequest request)
		{
			try
			{
				var json = request.RequiredPostJson();
				var settings = DynamicJson.Parse(json);

				var metadata = new Metadata { Creator = settings.creator };
				metadata.SetCopyrightNotice(settings.copyrightYear, settings.copyrightHolder);

				if (settings.licenseInfo.licenseType == "creativeCommons")
				{
					metadata.License = new CreativeCommonsLicense(
						true,
						settings.licenseInfo.creativeCommonsInfo.allowCommercial == "yes",
						GetDerivativeRule(settings.licenseInfo.creativeCommonsInfo.allowDerivatives))
					{
						IntergovernmentalOriganizationQualifier = settings.licenseInfo.creativeCommonsInfo.intergovernmentalVersion
					};
				}
				else if (settings.licenseInfo.licenseType == "contact")
					metadata.License = new NullLicense();
				else
					metadata.License = new CustomLicense();

				metadata.License.RightsStatement = settings.licenseInfo.rightsStatement;

				Model.ChangeBookLicenseMetaData(metadata);
			}
			catch(Exception ex)
			{
				SIL.Reporting.ErrorReport.NotifyUserOfProblem(ex, "There was a problem recording your changes to the copyright and license.");
			}
			request.PostSucceeded();
		}
	}
}
