/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web
{
	public class SurveyReporter
	{
		private Entity _survey;

		private IEnumerable<Entity> _surveyChoiceQuestions;
		private IEnumerable<Entity> _surveyTextQuestions;
		private List<Entity> _surveySubmissions;
		private Entity[] _surveyContacts;
		private IEnumerable<Entity>[] _submissionTextAnswers;
		private IEnumerable<Entity>[] _submissionChoiceAnswers;

		public string ContextName { get; set; }

		public SurveyReporter(Guid surveyID)
		{
			var crm = CrmConfigurationManager.CreateContext(ContextName);

			var site = PortalContext.Current.Website;

			_survey = crm.CreateQuery("adx_survey").Where(
					survey =>
					survey.GetAttributeValue<Guid?>("adx_websiteid") == site.GetAttributeValue<Guid?>("adx_websiteid")).
					Where(survey => survey.GetAttributeValue<Guid?>("adx_surveyid") == surveyID).First();

			_surveyChoiceQuestions = _survey.GetRelatedEntities(crm, "adx_survey_choicequestion");
			_surveyTextQuestions = _survey.GetRelatedEntities(crm, "adx_survey_textareaquestion");
			var subs = _survey.GetRelatedEntities(crm, "adx_survey_surveysubmission");

			if (subs != null)
			{
				_surveySubmissions = subs.ToList();
			}

			if (_surveyChoiceQuestions == null && _surveyTextQuestions == null)
			{
				throw new ApplicationException("Survey Contains no questions.");
			}

			_surveyContacts = new Entity[_surveySubmissions.Count];
			_submissionTextAnswers = new IEnumerable<Entity>[_surveySubmissions.Count];
			_submissionChoiceAnswers = new IEnumerable<Entity>[_surveySubmissions.Count];
			//int index = 0;
			for (int index = 0; index < _surveySubmissions.Count; index++)
			{
				//_surveyContacts.Add(new CrmEntity());
				var contact = _surveySubmissions[index].GetRelatedEntity(crm, "adx_contact_surveysubmission");
				_surveyContacts[index] = contact;

				var thisSubmissionTextAnswers = _surveySubmissions[index].GetRelatedEntities(crm, "adx_surveysubmission_surveytextanswer");
				var thisSubmissionChoiceAnswers = _surveySubmissions[index].GetRelatedEntities(crm, "adx_surveysubmission_choiceanswer");

				_submissionTextAnswers[index] = thisSubmissionTextAnswers;
				_submissionChoiceAnswers[index] = thisSubmissionChoiceAnswers;
			}

		}

		public void Write(HtmlTextWriter writer)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Table);

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.RenderBeginTag(HtmlTextWriterTag.Th);
			writer.Write("Submission name");
			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Th);
			writer.Write("Visitor ID");
			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Th);
			writer.Write("Contact Name");
			writer.RenderEndTag();

			foreach (var question in _surveyChoiceQuestions)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Th);
				writer.Write(question.GetAttributeValue<string>("adx_name"));
				writer.RenderEndTag();

				writer.RenderBeginTag(HtmlTextWriterTag.Th);
				writer.Write(question.GetAttributeValue<string>("adx_question"));
				writer.RenderEndTag();

			}

			foreach (var question in _surveyTextQuestions)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Th);
				writer.Write(question.GetAttributeValue<string>("adx_name") + " : " + question.GetAttributeValue<string>("adx_question"));
				writer.RenderEndTag();

			}

			writer.RenderEndTag();

			if (_surveySubmissions == null)
			{
				writer.RenderEndTag();
				return;
			}


			for (int i = 0; i < _surveySubmissions.Count; i++)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);
				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				writer.Write(_surveySubmissions[i].GetAttributeValue<string>("adx_name"));
				writer.RenderEndTag();

				var contact = _surveyContacts[i];

				if (contact != null)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
					writer.Write(@"&nbsp;");
					writer.RenderEndTag();
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
					writer.Write(contact.GetAttributeValue<string>("fullname"));
					writer.RenderEndTag();
				}
				else
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
					writer.Write(_surveySubmissions[i].GetAttributeValue<string>("adx_visitorid"));
					writer.RenderEndTag();
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
					writer.Write(@"&nbsp;");
					writer.RenderEndTag();
				}

				var thisSubmissionTextAnswers = _submissionTextAnswers[i];
				var thisSubmissionChoiceAnswers = _submissionChoiceAnswers[i];

				var crm = CrmConfigurationManager.CreateContext(ContextName);

				foreach (var question in _surveyChoiceQuestions)
				{
					bool answered = false;
					foreach (var answer in thisSubmissionChoiceAnswers)
					{
						var anses = crm.CreateQuery("adx_surveychoiceanswer");
						Entity answer1 = answer;
						var ans = anses.Where(a => a.GetAttributeValue<Guid>("adx_surveychoiceanswerid") == answer1.GetAttributeValue<Guid>("adx_surveychoiceanswerid")).First();
						if (ans.GetRelatedEntity(crm, "adx_choicequestion_choiceanswer").GetAttributeValue<Guid?>("adx_surveychoicequestionid")
							== question.GetAttributeValue<Guid?>("adx_surveychoicequestionid"))
						{
							string partialChoiceList = string.Empty;
							var choices = ans.GetRelatedEntities(crm, "adx_surveychoiceanswer_surveychoice");
							foreach (var entity in choices)
							{
								partialChoiceList = partialChoiceList + entity.GetAttributeValue<string>("adx_name") + "|";
							}
							string pattern = @"\|$";
							string replacement = " ";

							writer.RenderBeginTag(HtmlTextWriterTag.Td);

							if (partialChoiceList != null)
							{
								string result = Regex.Replace(partialChoiceList, pattern, replacement);
								writer.Write(result);
							}
							else
							{
								writer.Write(partialChoiceList);
							}
							writer.RenderEndTag();

							writer.RenderBeginTag(HtmlTextWriterTag.Td);

							string input = ans.GetAttributeValue<string>("adx_answer");
							
							if (input != null)
							{
								string result = Regex.Replace(input, pattern, replacement);
								writer.Write(result);
							}
							else
							{
								writer.Write(input);
							}
							writer.RenderEndTag();
							answered = true;
						}
					}
					if (!answered)
					{
						writer.RenderBeginTag(HtmlTextWriterTag.Td);
						writer.Write(@"&nbsp;");
						writer.RenderEndTag();
					}
				}
				foreach (var question in _surveyTextQuestions)
				{
					bool answered = false;
					foreach (var answer in thisSubmissionTextAnswers)
					{
						var anses = crm.CreateQuery("adx_surveytextanswer");
						Entity answer1 = answer;
						var ans = anses.Where(a => a.GetAttributeValue<Guid>("adx_surveytextanswerid") == answer1.GetAttributeValue<Guid>("adx_surveytextanswerid")).First();
						if (ans.GetRelatedEntity(crm, "adx_textareaquestion_textanswer").GetAttributeValue<Guid?>("adx_surveytextareaquestionid")
							== question.GetAttributeValue<Guid?>("adx_surveytextareaquestionid"))
						{
							writer.RenderBeginTag(HtmlTextWriterTag.Td);
							if (string.IsNullOrEmpty(ans.GetAttributeValue<string>("adx_answer")))
							{
								writer.Write(@"&nbsp;");
							}
							else
							{
								writer.Write(ans.GetAttributeValue<string>("adx_answer"));
							}
							//writer.Write(answer.GetAttributeValue<string>("adx_answer"));
							writer.RenderEndTag();
							answered = true;
						}
					}
					if (!answered)
					{
						writer.RenderBeginTag(HtmlTextWriterTag.Td);
						writer.Write(@"&nbsp;");
						writer.RenderEndTag();
					}
				}

				writer.RenderEndTag();

			}

			writer.RenderEndTag();
			writer.Flush();



		}



	}
}
