using MQTTnet.Client;
using MQTTnet;
using Timer = System.Timers.Timer;
using System.Timers;
using Microsoft.Extensions.Options;

namespace IO_Link
{
    public class ManagedMqtt
    {
		public IP_Model Options { get; set; }
		public bool IsConnected => _mqttClient is not null && _mqttClient.IsConnected;

        public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceived;
        public event Func<MqttClientDisconnectedEventArgs, Task>? Disconnected;
        private readonly Timer _reconnectTimer;

        private IMqttClient? _mqttClient;
        public ManagedMqtt(IOptions<IP_Model> options)
        {
			Options = options.Value;
			_reconnectTimer = new Timer(5000);
            _reconnectTimer.Elapsed += OnReconnectTimerElapsed;         
        }


        private async void OnReconnectTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            await ConnectAsync(Options.IP1);
        }

        public async Task ConnectAsync(string IP)
        {
            _reconnectTimer.Enabled = false;
            if (_mqttClient is not null)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
            }

            _mqttClient = new MqttFactory().CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(IP, 1883)
                .WithTimeout(TimeSpan.FromSeconds(5))
                .WithClientId(Guid.NewGuid().ToString())
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(1));

            _mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceived;
            _mqttClient.DisconnectedAsync += OnDisconnected;

            

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var result = await _mqttClient.ConnectAsync(mqttClientOptions.Build(), timeout.Token);

            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                _reconnectTimer.Enabled = true;
            }
            else
            {
                await _mqttClient.SubscribeAsync("VTSauto/AR_project/IOT_pub/IO");
                


            }
        }
        private async Task OnDisconnected(MqttClientDisconnectedEventArgs eventArgs)
        {
            await ConnectAsync(Options.IP1);
        }

        public async Task DisconnectedAsync()
        {
            await _mqttClient.DisconnectAsync();
        }

        public async Task Subscribe(string topic)
        {
            if (_mqttClient is null)
            {
                throw new InvalidOperationException("MQTT Client is not connected.");
            }

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topicFilter)
                .Build();

            var result = await _mqttClient.SubscribeAsync(subscribeOptions);

            foreach (var subscription in result.Items)
            {
                if (subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0 &&
                    subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS1 &&
                    subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS2)
                {
                    Console.WriteLine($"MQTT Client Subscription {subscription.TopicFilter.Topic} Failed: {subscription.ResultCode}");
                }
            }
        }

        public async Task Publish(string topic, string payload, bool retainFlag)
        {
            if (_mqttClient is null)
            {
                throw new InvalidOperationException("MQTT Client is not connected.");
            }

            var applicationMessageBuilder = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithRetainFlag(retainFlag)
                .WithPayload(payload);

            var applicationMessage = applicationMessageBuilder.Build();

            var result = await _mqttClient.PublishAsync(applicationMessage);

            if (result.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                Console.WriteLine($"MQTT Client Publish {applicationMessage.Topic} Failed: {result.ReasonCode}");
            }
        }
    }
}
