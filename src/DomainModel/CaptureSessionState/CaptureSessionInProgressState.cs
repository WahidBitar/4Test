﻿using System;

namespace DomainModel
{
    class CaptureSessionInProgressState : CaptureSessionStateAbstraction
    {
        public CaptureSessionInProgressState(CaptureSession context) : base(context)
        {
        }

        public override CaptureSessionState State
        {
            get { return CaptureSessionState.InProgress; }
        }

        public override void Proceed()
        {
            if (context.canAddRequest())
                context.setState(new CaptureSessionInProgressState(context));
            else
                base.Proceed();
        }

        public override void Finish()
        {
            if (context.canFinish())
                context.setState(new CaptureSessionFinishedState(context));
            else
                base.Finish();
        }

        public override void Cancel()
        {
            if (context.canCancel())
                context.setState(new CaptureSessionCancelledState(context));
            else
                base.Cancel();
        }
    }
}