﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using Emgu.CV.Util;

using VVVV.CV.Core;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils;

using VVVV.Nodes.Generic;



namespace VVVV.CV.Nodes
{
    #region PluginInfo
    [FilterInstance("Queue", Version = "FIFO", Tags = "", Help = "A Queue for CV.Image", Author = "sebl")]
    #endregion PluginInfo
    public class QueueNode : IFilterInstance
    {
        #region fields and pins

        [Input("Frame Count")]
        public int FFrameCount = 1;

        [Input("Insert")]
        public bool FDoInsert = false;

        [Input("Reset")]
        public bool FReset = false;

        [Input("Index")]
        public int FInIndex = 0;


        [Output("Queue Count")]
        public int FOutCount;

        [Output("Images Per Frame")]
        public int FOutFrames;

        int QueueCount = 0;

        private List<CVImage> FrameQueue = new List<CVImage>();

        private CVImage LastImage;

        int FFrames = 0;
        public int Frames
        {
            get
            {
                int f = FFrames;
                FFrames = 0;
                return f;
            }
        }

        #endregion fields and pins


        public override void Allocate()
        {
            FrameQueue = new List<CVImage>();
            FOutput.Image.Initialise(FInput.Image.ImageAttributes);
            LastImage.Initialise(FInput.Image.ImageAttributes);
        }

        public override bool IsFast()
        {
            return false;
        }

        void Flagger()
        {
            FlagForProcess();
        }

        public override void Process()
        {
            if (LastImage != FInput.Image)
            {

            }

            if (FReset)
            {
                foreach (CVImage img in FrameQueue)
                    img.Dispose();
                
                FrameQueue.Clear();
            }

            // push frames
            
            if (!FInput.LockForReading()) // not sure if needed
                return;    

            if (FDoInsert)
            {
                CVImage copy = new CVImage();
                copy.Initialise(FInput.ImageAttributes);
                ImageUtils.CopyImage(FInput.Image, copy);
                FrameQueue.Insert(0, copy);
                //FrameQueue.Add(copy);
            }

            LastImage = FInput.Image;

            FInput.ReleaseForReading(); // not sure if needed


            // trim queue

            if (FFrameCount >= 0 && FrameQueue.Count > FFrameCount)
            {
                var tooMuch = FrameQueue.Count - FFrameCount;
                var toDispose = FrameQueue.GetRange(FFrameCount, tooMuch);
                foreach (CVImage img in toDispose)
                    img.Dispose();

                FrameQueue.RemoveRange(FFrameCount, tooMuch);
                //FrameQueue.RemoveRange(0, tooMuch);
            }

            QueueCount = FrameQueue.Count();

            // set OutPuts

            FOutCount = QueueCount;

            var idx = FInIndex % QueueCount;

            FOutput.Send(FrameQueue[idx]);
        }

    }

    #region PluginInfo
    [FilterInstance("Queue", Version = "LIFO", Tags = "", Help = "A Queue for CV.Image with Last-in, First Out", Author = "sebl")]
    #endregion PluginInfo
    public class QueueLifoNode : IFilterInstance
    {
        #region fields and pins

        [Input("Frame Count")]
        public int FFrameCount = 1;

        [Input("Insert")]
        public bool FDoInsert = false;

        [Input("Reset")]
        public bool FReset = false;

        [Input("Index")]
        public int FInIndex = 0;


        [Output("Queue Count")]
        public int FOutCount;

        [Output("Images Per Frame")]
        public int FOutFrames;

        int QueueCount = 0;

        private List<CVImage> FrameQueue = new List<CVImage>();

        private CVImage LastImage;

        int FFrames = 0;
        public int Frames
        {
            get
            {
                int f = FFrames;
                FFrames = 0;
                return f;
            }
        }

        #endregion fields and pins


        public override void Allocate()
        {
            FrameQueue = new List<CVImage>();
            FOutput.Image.Initialise(FInput.Image.ImageAttributes);
            LastImage.Initialise(FInput.Image.ImageAttributes);
        }

        public override bool IsFast()
        {
            return false;
        }

        void Flagger()
        {
            FlagForProcess();
        }

        public override void Process()
        {
            if (LastImage != FInput.Image)
            {

            }

            if (FReset)
            {
                foreach (CVImage img in FrameQueue)
                    img.Dispose();

                FrameQueue.Clear();
            }

            // push frames

            if (!FInput.LockForReading()) // not sure if needed
                return;

            if (FDoInsert)
            {
                CVImage copy = new CVImage();
                copy.Initialise(FInput.ImageAttributes);
                ImageUtils.CopyImage(FInput.Image, copy);
                //FrameQueue.Insert(0, copy);
                FrameQueue.Add(copy);
            }

            LastImage = FInput.Image;

            FInput.ReleaseForReading(); // not sure if needed


            // trim queue

            if (FFrameCount >= 0 && FrameQueue.Count > FFrameCount)
            {
                var tooMuch = FrameQueue.Count - FFrameCount;
                var toDispose = FrameQueue.GetRange(FFrameCount, tooMuch);
                foreach (CVImage img in toDispose)
                    img.Dispose();

                //FrameQueue.RemoveRange(FFrameCount, tooMuch);
                FrameQueue.RemoveRange(0, tooMuch);
            }

            QueueCount = FrameQueue.Count();

            // set OutPuts

            FOutCount = QueueCount;

            var idx = FInIndex % QueueCount;

            FOutput.Send(FrameQueue[idx]);
        }

    }
}
