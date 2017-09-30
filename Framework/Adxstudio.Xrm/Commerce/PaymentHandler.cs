/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Commerce
{
	public abstract class PaymentHandler : IHttpHandler
	{
		protected PaymentHandler(string portalName)
		{
			PortalName = portalName;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public string PortalName { get; private set; }

		public void ProcessRequest(HttpContext context)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(PortalName, context.Request.RequestContext);

			Tuple<Guid, string> quoteAndReturnUrl;
			
			if (!TryGetQuoteAndReturnUrl(context.Request, dataAdapterDependencies, out quoteAndReturnUrl))
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Unable to determine quote from request.");
			}

			try
			{
				var validation = ValidatePayment(context, dataAdapterDependencies, quoteAndReturnUrl);

				LogPaymentRequest(context, dataAdapterDependencies, quoteAndReturnUrl, validation);

				if (!validation.Success)
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, "Start");

                    HandleUnsuccessfulPayment(context, quoteAndReturnUrl, validation.ErrorMessage);

					return;
				}

                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Payment sucessful.");

				string receiptNumber;

				if (!TryGetReceiptNumber(context, out receiptNumber))
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Failed to get receipt number.");
				}

				var quoteid = quoteAndReturnUrl.Item1;
				var serviceContext = dataAdapterDependencies.GetServiceContext();
				var salesorder =
					serviceContext.CreateQuery("salesorder")
						.FirstOrDefault(
							s =>
								s.GetAttributeValue<EntityReference>("quoteid") != null &&
								s.GetAttributeValue<EntityReference>("quoteid").Equals(new EntityReference("quote", quoteid)));

				if (salesorder == null)
				{
					ConvertQuoteToOrder(context, dataAdapterDependencies, quoteid, receiptNumber);
				}

				HandleSuccessfulPayment(context, quoteAndReturnUrl);
			}
			catch (Exception e)
			{
				LogPaymentRequest(context, dataAdapterDependencies, quoteAndReturnUrl, e);

				throw;
			}
		}

		protected virtual void ConvertQuoteToOrder(HttpContext context, PortalConfigurationDataAdapterDependencies dataAdapterDependencies, Guid quoteId, string receiptNumber)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var quoteEntityReference = new EntityReference("quote", quoteId);
			var serviceContext = dataAdapterDependencies.GetServiceContextForWrite();
			var quote = serviceContext.CreateQuery("quote").FirstOrDefault(q => q.GetAttributeValue<Guid>("quoteid") == quoteId);

			if (quote == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Quote could not be found with id '{0}'. Sales Order could not be created.", quoteId));
				
				return;
			}

			var status = quote.GetAttributeValue<OptionSetValue>("statecode");

			if (status == null || status.Value != 0)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Quote with id '{0}' statecode '{1}' is not in Draft state. Sales Order could not be created.", quoteId, status == null ? "null" : status.Value.ToString(CultureInfo.InvariantCulture)));

				return;
			}

			// Activate the quote (should be in Draft status [statecode=0]).
			serviceContext.Execute(new SetStateRequest
			{
				EntityMoniker = quoteEntityReference,
				State = new OptionSetValue(1),
				Status = new OptionSetValue(2)
			});

			var quoteClose = new Entity("quoteclose");

			quoteClose["subject"] = "Payment";
			quoteClose["quoteid"] = quoteEntityReference;

			// Win the quote (required before converting to order).
			serviceContext.Execute(new WinQuoteRequest
			{
				QuoteClose = quoteClose,
				Status = new OptionSetValue(-1),
			});

			// Convert the quote to an order.
			var convertQuoteToSalesOrderResponse = (ConvertQuoteToSalesOrderResponse)serviceContext.Execute(new ConvertQuoteToSalesOrderRequest
			{
				ColumnSet = new ColumnSet("salesorderid"),
				QuoteId = quoteEntityReference.Id,
			});

			if (convertQuoteToSalesOrderResponse != null && !string.IsNullOrWhiteSpace(receiptNumber))
			{
				var order = convertQuoteToSalesOrderResponse.Entity;

				if (order != null)
				{
					var orderUpdate = new Entity("salesorder") { Id = order.Id };
					orderUpdate["adx_receiptnumber"] = receiptNumber;
					serviceContext.Attach(orderUpdate);
					serviceContext.UpdateObject(orderUpdate);
					serviceContext.SaveChanges();
				}
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");
		}

		protected abstract void HandleSuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl);

		protected abstract void HandleUnsuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl, string errorMessage);

		protected virtual void LogPaymentRequest(HttpContext context, PortalConfigurationDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl, string subject, string log)
		{
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var note = new Annotation
			{
				Subject = subject,
				Regarding = new EntityReference("quote", quoteAndReturnUrl.Item1),
				FileAttachment = AnnotationDataAdapter.CreateFileAttachment("log.txt", "text/plain", Encoding.UTF8.GetBytes(log))
			};
			dataAdapter.CreateAnnotation(note);
		}

		protected virtual void LogPaymentRequest(HttpContext context, PortalConfigurationDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl, Exception exception)
		{
			var log = new StringBuilder();

			log.AppendLine(GetType().FullName).AppendLine().AppendLine();
			log.AppendFormat("Exception: {0}", exception);

			LogPaymentRequest(context, dataAdapterDependencies, quoteAndReturnUrl, "Payment Exception", log.ToString());
		}

		protected virtual void LogPaymentRequest(HttpContext context, PortalConfigurationDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl, IPaymentValidation validation)
		{
			var log = new StringBuilder();

			log.AppendLine(GetType().FullName).AppendLine();
			log.AppendLine(validation.Log);

			if (!string.IsNullOrEmpty(validation.ErrorMessage))
			{
				log.AppendLine().AppendFormat("Error message: {0}", validation.ErrorMessage).AppendLine();
			}

			LogPaymentRequest(
				context,
				dataAdapterDependencies,
				quoteAndReturnUrl,
				"Payment Log ({0})".FormatWith(validation.Success ? "Successful" : "Unsuccessful"),
				log.ToString());
		}

		protected abstract bool TryGetQuoteAndReturnUrl(HttpRequest request, IDataAdapterDependencies dataAdapterDependencies, out Tuple<Guid, string> quoteAndReturnUrl);

		protected abstract IPaymentValidation ValidatePayment(HttpContext context, IDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl);

		protected interface IPaymentValidation
		{
			string ErrorMessage { get; }

			string Log { get; }

			bool Success { get; }
		}

		protected abstract class PaymentValidation : IPaymentValidation
		{
			protected PaymentValidation(bool success, string log, string errorMessage = null)
			{
				Success = success;
				Log = log;
				ErrorMessage = errorMessage;
			}

			public string ErrorMessage { get; private set; }

			public string Log { get; private set; }

			public bool Success { get; private set; }
		}

		protected class SuccessfulPaymentValidation : PaymentValidation
		{
			public SuccessfulPaymentValidation(string log, string errorMessage = null) : base(true, log, errorMessage) { }
		}

		protected class UnsuccessfulPaymentValidation : PaymentValidation
		{
			public UnsuccessfulPaymentValidation(string log, string errorMessage) : base(false, log, errorMessage) { }
		}

		protected abstract bool TryGetReceiptNumber(HttpContext context, out string receiptNumber);
	}
}
