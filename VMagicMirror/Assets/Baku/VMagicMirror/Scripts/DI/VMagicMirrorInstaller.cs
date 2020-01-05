using UnityEngine;
using Zenject;

namespace Baku.VMagicMirror
{
    public class VMagicMirrorInstaller : MonoInstaller
    {
        [SerializeField] private ReceivedMessageHandler messageHandler = null;
        [SerializeField] private VRMLoadController loadController = null;
        [SerializeField] private MmfServer mmfServer = null;

        [SerializeField] private RawInputChecker rawInputChecker = null;
        [SerializeField] private FaceTracker faceTracker = null;
        [SerializeField] private MidiInputObserver midiInputObserver = null;
        [SerializeField] private StatefulXinputGamePad gamepad = null;
        
        [SerializeField] private DeformableCounter deformableCounterPrefab = null; 

        public override void InstallBindings()
        {
            //メッセージハンドラの依存はここで注入(偽レシーバを入れたい場合、interfaceを切って別インスタンスを捻じ込めばOK)
            Container.BindInstance(messageHandler);

            //入力監視系のコードはメッセージハンドラと同格くらいに扱えそうなので、ここでバインドする
            Container.BindInstance(rawInputChecker);
            Container.BindInstance(faceTracker);
            Container.BindInstance(midiInputObserver);
            Container.BindInstance(gamepad);

            //Deformを使うオブジェクトがここを参照することで、DeformableManagerを必要な時だけ動かせるようにする
            Container.Bind<DeformableCounter>()
                .FromComponentInNewPrefab(deformableCounterPrefab)
                .AsSingle();
            
            //VRMLoadControllerがIVRMLoadable(VRMのロード/破棄イベント送信元)の実装を提供する
            Container
                .Bind<IVRMLoadable>()
                .FromInstance(loadController)
                .AsSingle();

            //プロセス間通信の送り手はMemoryMappedFileベースのIPCでやる
            Container
                .Bind<IMessageSender>()
                .FromInstance(mmfServer)
                .AsSingle();
            
        }
    }
}
