using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Infrastructure.WcfInspectors
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            // Opcional: puedes ver también lo que envías
            // Console.WriteLine("=== REQUEST ===");
            // Console.WriteLine(request.ToString());
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            Console.WriteLine("=== RESPUESTA CRUDA DEL SRI ===");
            // Crear una copia del mensaje para poder leerlo sin consumirlo
            MessageBuffer buffer = reply.CreateBufferedCopy(int.MaxValue);
            reply = buffer.CreateMessage(); // restaurar el mensaje original
            using (var reader = buffer.CreateMessage().GetReaderAtBodyContents())
            {
                string xml = reader.ReadOuterXml();
                Console.WriteLine(xml);
            }
            Console.WriteLine("===================================");
        }
    }
}