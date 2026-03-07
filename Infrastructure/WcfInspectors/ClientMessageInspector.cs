using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Infrastructure.WcfInspectors
{
    public static class MessageInspectorStorage
    {
        private static string _lastResponseXml;
        private static readonly object _lock = new object();

        public static string LastResponseXml
        {
            get { lock (_lock) return _lastResponseXml; }
            set { lock (_lock) _lastResponseXml = value; }
        }
    }

    public class ClientMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // Guardar el XML de la respuesta antes de que sea consumido
            MessageBuffer buffer = reply.CreateBufferedCopy(int.MaxValue);
            reply = buffer.CreateMessage(); // restaurar el mensaje original
            using (var reader = buffer.CreateMessage().GetReaderAtBodyContents())
            {
                string xml = reader.ReadOuterXml();
                MessageInspectorStorage.LastResponseXml = xml;
                System.Console.WriteLine("=== RESPUESTA CRUDA DEL SRI ===");
                System.Console.WriteLine(xml);
                System.Console.WriteLine("===================================");
            }
        }
    }
}