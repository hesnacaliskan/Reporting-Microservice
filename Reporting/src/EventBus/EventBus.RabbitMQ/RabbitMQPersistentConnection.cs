using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {
        private readonly  IConnectionFactory connectionFactory ;
        private readonly int retryCount;
        private IConnection connection;//Hangi connection'ın açık olup olmıycağı bilgisini tutan interface
        //Iconnection için nuget paketlerinden RabbitMQ.Client yüklenmeli
        private object lock_object = new object();// Aynı proje içerisinde birden fazla tryconnect çağırılabilir.
        //TryConnect() içinde kullanılıyor.
        private bool disposed;
        private bool _disposed;

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retrycount = 5)
            // connection nesnesini oluşturmak için connection factory'e ihtiyacımız var.
        {
            this.connectionFactory = connectionFactory; 
        }   

        public bool IsConnected => connection != null && connection.IsOpen;// Connection aktif mi değil mi ? 
        // Ve null değerden başka bir değer olmalı.

        public EventHandler<ShutdownEventArgs> Connection_ConnetionShutdown { get; private set; }
        public EventHandler<CallbackExceptionEventArgs> Connetion_CallbackException { get; private set; }

        public IModel CreateModel()
        {
            return connection.CreateModel();
        }
            


        public void Dispose()
        {
            _disposed = true;
            connection?.Dispose();  
        }



        public bool TryConnect()// RabbitMQ ile bağlantıya geçmek için.
        {
            lock (lock_object)// aynı metot tekrar geldiğinde bir önceki işlemin bitmesini bekliycek.
            {
                var policy = Policy.Handle<SocketException>()// Polly paketi kurulmalı.Retry mekanızması kurulucak.
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(retryCount, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), (ex, time) =>
                    {
                    }
                );

                policy.Execute(() =>
                {

                    connection = connectionFactory.CreateConnection();// connection oluştu.

                });
                if (IsConnected)// Bağlanabildiysek true, bağlanamadıysak false döner.

                {
                    connection.ConnectionShutdown += Connection_ConnectionShutdown; ;// Oluşturduğumuz bağlantının sürekli olarak bağlı kalabil-
                                                                                  // mesini sağlamak için bazı eventleri dinleyebiliriz.
                    connection.CallbackException += Connection_CallbackException;
                    connection.ConnectionBlocked += Connection_ConnectionBlocked;
                      
                    // log atılabilir.

                    return true;


                }
                return false;
            }
        }

        private void Connection_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Connection_ConnectionBlocked(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            if (_disposed) return; // disposed edildiyse tekrar bağlanmayı denemesin başa dönsün.     
            TryConnect();
        }
        private void Connection_CallbackException(object sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            if (_disposed) return; // disposed edildiyse tekrar bağlanmayı denemesin başa dönsün.
            TryConnect();
        }

        private void Connection_ShutDown(object sender, ShutdownEventArgs e)
        {
            // log ConnectionShutDown 

            if (_disposed) return; 

            TryConnect();// Koptuğu zaman bağlantı yine denemeye devam edicek.
        }
    }
}
