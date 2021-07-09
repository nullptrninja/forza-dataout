using Core.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core {
    public class UdpListener<TDataModel> : IUDPListener, IObservable<TDataModel> {
        private readonly UdpClient mClient;
        private readonly ForzaTelemetryConfig mConfig;
        private readonly List<IObserver<TDataModel>> mObservers;
        private readonly IDataModelConverter<TDataModel> converter;
        private Thread mListenThread;

        public UdpListener(ForzaTelemetryConfig config, IDataModelConverter<TDataModel> converter) {
            this.mConfig = config;
            mObservers = new List<IObserver<TDataModel>>();
            this.converter = converter;

            // We can DI this later but there doesn't seem to be much value in doing so here
            mClient = new UdpClient(mConfig.ListenPort);
        }

        public IDisposable Subscribe(IObserver<TDataModel> observer) {
            if (!mObservers.Contains(observer)) {
                mObservers.Add(observer);
            }
            return new Unsubscriber<TDataModel>(observer, mObservers);
        }

        public void Start() {
            if (mListenThread != null) {
                mListenThread.Join();
                mListenThread = null;
            }

            mListenThread = new Thread(() => StartListening());
            mListenThread.Start();
        }

        private void StartListening() {
            try {
                while (true) {
                    var receiveDatagram = mClient.ReceiveAsync();

                    Task.WaitAny(receiveDatagram);

                    // Validate and convert
                    var receivedBytes = receiveDatagram.Result;
                    if (receivedBytes.Buffer.Length != 0) {
                        var dataModel = converter.Convert(receivedBytes.Buffer);

                        NotifyListeners(dataModel);
                    }
                }
            }
            finally {
                mClient.Close();
                NotifyComplete();
            }
        }

        private void NotifyListeners(TDataModel data) {
            foreach (var observer in mObservers) {
                observer.OnNext(data);
            }
        }

        private void NotifyComplete() {
            foreach (var observer in mObservers) {
                observer.OnCompleted();
            }
        }
    }
}
