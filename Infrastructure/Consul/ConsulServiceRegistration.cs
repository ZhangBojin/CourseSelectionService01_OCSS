﻿using Consul;

namespace CourseSelectionService01_OCSS.Infrastructure.Consul
{
    public class ConsulServiceRegistration(IConsulClient consulClient, IConfiguration configuration)
    {
        private readonly IConsulClient _consulClient = consulClient;
        private readonly IConfiguration _configuration = configuration;

        public async Task RegisterServiceAsync()
        {
            var registration = new AgentServiceRegistration
            {
                ID = "CourseSelectionService01",
                Name = "CourseSelectionService01",
                Address = _configuration.GetSection("Cpolar")["ip"],
                Port = 80,
                Tags = new[] { "admin", "v1" },
                Check = new AgentServiceCheck
                {
                    HTTP = $"http://{_configuration.GetSection("Cpolar")["ip"]}/Consul/Health",  // 健康检查的 HTTP 地址
                    Interval = TimeSpan.FromSeconds(10), // 健康检查的执行间隔
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(60)// 服务被标记为“临时不可用”后的注销时间
                }
            };

            await _consulClient.Agent.ServiceRegister(registration);
        }

        public async Task DeregisterServiceAsync()
        {
            await _consulClient.Agent.ServiceDeregister("CourseSelectionService01");
        }
    }
}
