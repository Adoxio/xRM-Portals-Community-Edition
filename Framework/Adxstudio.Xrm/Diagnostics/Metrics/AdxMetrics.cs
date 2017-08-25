/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adxstudio.Xrm.Diagnostics.Metrics
{
    /// <summary>
    /// Provides a way to send values to the MDM (Multidimensional metrics) system.
    /// </summary>
    /// <remarks>
    /// Five standard dimensions are added to metrics: Geo, Tenant, Org, PortalType and PortalId.
    /// </remarks>
    /// <example>
    /// The API could be used for calculating a rate of operations:
    /// <![CDATA[
    ///     long totalOperations = 0;
    ///     var myMetric = AdxMetrics.CreateMetric("Number of operations completed");
    ///     myMetric.LogValue(++totalOperations);
    /// ]]>
    /// Another example is reporting the latency of an operation, which can be used to
    /// create real time reports for min/max/avg/etc in a given context:
    /// <![CDATA[
    ///     var myMetric = AdxMetrics.CreateMetric("Execution time (milliseconds)");
    ///     ...
    ///     var startTime = DateTime.UtcNow;
    ///     DoSomeOperation();
    ///     var elapsed = DateTime.UtcNow - startTime;
    ///     myMetric.ReportValue((long)elapsed.TotalMilliseconds);
    /// ]]>
    /// </example>
    public static class AdxMetrics
    {
        private static MetricFactory metricFactory = (metricName) => new AdxMetric(metricName);

        /// <summary>
        /// Allows for unit testing metrics, by overriding metric creation mechanism.
        /// </summary>
        public static void OverrideMetricFactory(MetricFactory factoryMethod)
        {
            metricFactory = factoryMethod;
        }

        public delegate IAdxMetric MetricFactory(string metricName);

        /// <summary>
        /// Creates a named metric used to report data.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <returns>An <see cref="IAdxMetric"/> object that may be used to emit metric data.</returns>
        public static IAdxMetric CreateMetric(string metricName)
        {
            return metricFactory(metricName);
        }
    }

    /// <summary>
    /// Represents a single named metric.
    /// </summary>
    public interface IAdxMetric : IEquatable<IAdxMetric>
    {
        /// <summary>
        /// Name of the metric.
        /// </summary>
        string MetricName { get; }

        /// <summary>
        /// Report a value to MDM.
        /// </summary>
        /// <param name="rawValue">Metric value.</param>
        void LogValue(long rawValue);
    }
}
