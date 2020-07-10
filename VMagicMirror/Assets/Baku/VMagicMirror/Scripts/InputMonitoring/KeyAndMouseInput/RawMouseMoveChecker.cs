﻿using System;
using UnityEngine;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using Zenject;
using Baku.VMagicMirror.InterProcess;

namespace Baku.VMagicMirror
{
    /// <summary>
    /// RawInput的なマウス情報を返してくるやつ
    /// </summary>
    [RequireComponent(typeof(WindowProcedureHook))]
    public class RawMouseMoveChecker : MonoBehaviour
    {
        private WindowProcedureHook _windowProcedureHook = null;

        private int _dx;
        private int _dy;
        private readonly object _diffLock = new object();

        public bool EnableFpsAssumedRightHand { get; private set; } = false;

        /// <summary>
        /// 前回呼び出してからの間にマウス移動が積分された合計値を取得します。
        /// 読み出すことで累計値は0にリセットされます。
        /// </summary>
        /// <returns></returns>
        public (int dx, int dy) GetAndReset()
        {
            lock (_diffLock)
            {
                int x = _dx;
                int y = _dy;
                _dx = 0;
                _dy = 0;
                return (x, y);
            }
        }

        [Inject]
        public void Initialize(IMessageReceiver receiver)
        {
            receiver.AssignCommandHandler(
                VmmCommands.EnableFpsAssumedRightHand,
                c => EnableFpsAssumedRightHand = c.ToBoolean()
                );
        }
        
        private void Start()
        {
            //NOTE: このイベントはエディタ実行では飛んできません(Window Procedureに関わるので)
            _windowProcedureHook = GetComponent<WindowProcedureHook>();
            _windowProcedureHook.ReceiveRawInput += OnReceiveRawInput;
        }

        private void OnReceiveRawInput(IntPtr lParam)
        {
            if (RawInputData.FromHandle(lParam) is RawInputMouseData data &&
                data.Mouse.Flags.HasFlag(RawMouseFlags.MoveRelative))
            {
                AddDif(data.Mouse.LastX, data.Mouse.LastY);
            }
        }

        private void AddDif(int dx, int dy)
        {
            lock (_diffLock)
            {
                _dx += dx;
                _dy += dy;                
            }
        }
    }
}
