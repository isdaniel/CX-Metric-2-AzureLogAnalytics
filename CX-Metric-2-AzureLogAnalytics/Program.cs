// Create the DI container.
using CX_Metric_2_AzureLogAnalytics;
using Dapper;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Timers;
using System.Xml;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;


class Program
{

    static void Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<MySQLHelper>((sp) => {
            return new MySQLHelper("", "", "");
        });

        services.AddSingleton<LAWorkspace>((sp) => {
            return new LAWorkspace(sp.GetRequiredService<ILogger<LAWorkspace>>())
            {
                CustomerId = "",
                SharedKey = ""
            };
        });

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var aTimer = new System.Timers.Timer(30000);
        aTimer.Elapsed += new ElapsedEventHandler((s, e) => {
            OnTimedEvent(serviceProvider);
        });
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
        Console.ReadKey();
    }

    static void OnTimedEvent(IServiceProvider serviceProvider)
    {

        MySQLHelper mySQLHelper = serviceProvider.GetRequiredService<MySQLHelper>();
        var logAnalyticsWorkspace = serviceProvider.GetRequiredService<LAWorkspace>();
        string sql = "SELECT * \r\nFROM performance_schema.global_status  \r\nWHERE variable_name LIKE '%buffer%';";

        var result = mySQLHelper.GetData<ParameterModel>(sql);

        InnoDBMemroyModel m = new InnoDBMemroyModel();

        foreach (var item in result)
        {
            var prop = m.GetType().GetProperty(item.VARIABLE_NAME);

            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(m, Convert.ChangeType(item.VARIABLE_VALUE,prop.PropertyType));
            }
        }

        string json = JsonConvert.SerializeObject(m);
        Console.WriteLine($"{DateTime.Now} : {json}");
        logAnalyticsWorkspace.InjestLog(json, "MySQLInnoDBMemory");
    }

    public class InnoDBMemroyModel
    {
        public string Innodb_buffer_pool_dump_status { get; set; }
        public string Innodb_buffer_pool_load_status { get; set; }
        public string Innodb_buffer_pool_resize_status { get; set; }
        public decimal Innodb_buffer_pool_pages_data { get; set; }
        public decimal Innodb_buffer_pool_bytes_data { get; set; }
        public decimal Innodb_buffer_pool_pages_dirty { get; set; }
        public decimal Innodb_buffer_pool_bytes_dirty { get; set; }
        public decimal Innodb_buffer_pool_pages_flushed { get; set; }
        public decimal Innodb_buffer_pool_pages_free { get; set; }
        public decimal Innodb_buffer_pool_pages_misc { get; set; }
        public decimal Innodb_buffer_pool_pages_total { get; set; }
        public decimal Innodb_buffer_pool_read_ahead_rnd { get; set; }
        public decimal Innodb_buffer_pool_read_ahead { get; set; }
        public decimal Innodb_buffer_pool_read_ahead_evicted { get; set; }
        public decimal Innodb_buffer_pool_read_requests { get; set; }
        public decimal Innodb_buffer_pool_reads { get; set; }
        public decimal Innodb_buffer_pool_wait_free { get; set; }
        public decimal Innodb_buffer_pool_write_requests { get; set; }
        public decimal Innodb_buffer_disk_reads { get; set; }
        public decimal Innodb_buffer_disk_read_conflicts { get; set; }
        public decimal Innodb_buffer_disk_writes { get; set; }
        public decimal Innodb_buffer_disk_writes_conflicts { get; set; }
        public decimal Innodb_buffer_disk_removes { get; set; }
    }
}


