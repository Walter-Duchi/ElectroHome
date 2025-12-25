using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.TryParse(emailSettings["SmtpPort"], out var port) ? port : 587;
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];
            var enableSsl = bool.TryParse(emailSettings["EnableSsl"], out var ssl) ? ssl : true;

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
            {
                _logger.LogError("Credenciales de correo no configuradas en appsettings.json");
                return false;
            }

            try
            {
                _logger.LogInformation($"Iniciando envío de correo de restablecimiento a: {toEmail}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sistema de Reclamos", senderEmail));
                message.To.Add(new MailboxAddress(userName, toEmail));
                message.Subject = "Restablecimiento de Contraseña - Sistema de Reclamos";
                message.Date = DateTimeOffset.Now.DateTime;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = CreateResetPasswordEmailHtml(userName, resetLink),
                    TextBody = CreateResetPasswordEmailText(userName, resetLink)
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Configurar timeout
                client.Timeout = 30000; // 30 segundos

                _logger.LogInformation($"Conectando a {smtpServer}:{smtpPort}...");

                await client.ConnectAsync(
                    smtpServer,
                    smtpPort,
                    enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None
                );

                _logger.LogInformation("Conectado exitosamente, autenticando...");

                // Autenticar usando las credenciales (App Password)
                await client.AuthenticateAsync(senderEmail, senderPassword);

                _logger.LogInformation("Autenticación exitosa, enviando correo...");

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Correo enviado exitosamente a {toEmail}");
                return true;
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, "Error de autenticación con Gmail. Verifica:");
                _logger.LogError("- Que la contraseña de aplicación sea correcta");
                _logger.LogError("- Que la verificación en dos pasos esté activada");
                _logger.LogError("- Que hayas usado la contraseña de aplicación, no la contraseña normal");
                return false;
            }
            catch (SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx, $"Error SMTP al enviar correo: {smtpEx.Message}");
                _logger.LogError($"Status code: {smtpEx.StatusCode}, Error code: {smtpEx.ErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al enviar correo a {toEmail}: {ex.Message}");
                return false;
            }
        }

        private string CreateResetPasswordEmailHtml(string userName, string resetLink)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Restablecimiento de Contraseña</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #ffffff;
                        border-radius: 10px;
                        overflow: hidden;
                        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        padding: 30px 20px;
                        text-align: center;
                    }}
                    .header h1 {{
                        color: white;
                        margin: 0;
                        font-size: 24px;
                        font-weight: 600;
                    }}
                    .content {{
                        padding: 40px 30px;
                    }}
                    .greeting {{
                        color: #2d3748;
                        font-size: 18px;
                        margin-bottom: 20px;
                    }}
                    .message {{
                        color: #4a5568;
                        font-size: 16px;
                        margin-bottom: 30px;
                        line-height: 1.8;
                    }}
                    .button-container {{
                        text-align: center;
                        margin: 40px 0;
                    }}
                    .reset-button {{
                        display: inline-block;
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        padding: 16px 32px;
                        text-decoration: none;
                        border-radius: 8px;
                        font-weight: 600;
                        font-size: 16px;
                        transition: transform 0.3s ease, box-shadow 0.3s ease;
                    }}
                    .reset-button:hover {{
                        transform: translateY(-2px);
                        box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
                    }}
                    .expiry-note {{
                        color: #718096;
                        font-size: 14px;
                        margin-top: 20px;
                        padding: 15px;
                        background-color: #f7fafc;
                        border-radius: 6px;
                        border-left: 4px solid #4299e1;
                    }}
                    .link-fallback {{
                        margin-top: 30px;
                        padding: 15px;
                        background-color: #edf2f7;
                        border-radius: 6px;
                        word-break: break-all;
                    }}
                    .footer {{
                        text-align: center;
                        padding: 20px;
                        color: #a0aec0;
                        font-size: 12px;
                        border-top: 1px solid #e2e8f0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Sistema de Reclamos</h1>
                    </div>
                    
                    <div class='content'>
                        <div class='greeting'>
                            <strong>Hola {userName},</strong>
                        </div>
                        
                        <div class='message'>
                            Has solicitado restablecer tu contraseña en el Sistema de Reclamos. 
                            Haz clic en el botón de abajo para crear una nueva contraseña segura.
                        </div>
                        
                        <div class='button-container'>
                            <a href='{resetLink}' class='reset-button' target='_blank'>
                                Restablecer Contraseña
                            </a>
                        </div>
                        
                        <div class='expiry-note'>
                            ⏰ <strong>Este enlace expirará en 15 minutos.</strong><br>
                            Si no solicitaste restablecer tu contraseña, puedes ignorar este correo con seguridad.
                        </div>
                        
                        <div class='link-fallback'>
                            <strong>Si el botón no funciona:</strong><br>
                            Copia y pega este enlace en tu navegador:<br>
                            <a href='{resetLink}' style='color: #4299e1;'>{resetLink}</a>
                        </div>
                    </div>
                    
                    <div class='footer'>
                        Este es un correo automático del Sistema de Reclamos. Por favor no respondas a este mensaje.<br>
                        © {DateTime.Now.Year} Sistema de Reclamos. Todos los derechos reservados.
                    </div>
                </div>
            </body>
            </html>";
        }

        private string CreateResetPasswordEmailText(string userName, string resetLink)
        {
            return $@"
            RESTABLECIMIENTO DE CONTRASEÑA - SISTEMA DE RECLAMOS
            
            Hola {userName},
            
            Has solicitado restablecer tu contraseña en el Sistema de Reclamos.
            
            Para crear una nueva contraseña, accede al siguiente enlace:
            {resetLink}
            
            ⚠️ IMPORTANTE:
            - Este enlace expirará en 15 minutos.
            - Si no solicitaste restablecer tu contraseña, ignora este correo.
            
            Si tienes problemas con el enlace, cópialo y pégalo manualmente en tu navegador.
            
            --
            Este es un correo automático. No respondas a este mensaje.
            © {DateTime.Now.Year} Sistema de Reclamos.";
        }
    }
}