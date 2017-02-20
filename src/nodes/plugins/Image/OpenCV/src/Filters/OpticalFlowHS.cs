﻿using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using VVVV.PluginInterfaces.V2;
using System;
using Emgu.CV.Structure;
using VVVV.CV.Core;

namespace VVVV.CV.Nodes
{
	[FilterInstance("OpticalFlow", Version = "HornSchunck", Help = "Perform HS optical flow on image", Author = "elliotwoods")]
	public class OpticalFlowHSInstance : IFilterInstance
	{
		private Size FSize;

		private CVImage FCurrent = new CVImage();
		private CVImage FPrevious = new CVImage();
		private CVImage FVelocityX = new CVImage();
		private CVImage FVelocityY = new CVImage();

        private CVImage FFlow = new CVImage();

        private double FLambda = 0.1;
		[Input("Lambda", DefaultValue = 0.1, MinValue = 0, MaxValue = 1)]
		public double Lambda
		{	set
			{
				if (value < 0)
					value = 0;

				if (value > 1)
					value = 1;

				FLambda = value;
			}
		}

		private int FIterations = 100;
		[Input("Maximum Iterations", DefaultValue = 100, MinValue = 1, MaxValue = 500)]
		public int Iterations
		{
			set
			{
				if (value < 1)
					value = 1;
				if (value > 500)
					value = 500;

				FIterations = value;
			}
		}

		[Input("Use Previous Velocity")]
		public bool UsePrevious = false;

		public override void Allocate()
		{
			FSize = FInput.ImageAttributes.Size;
			FOutput.Image.Initialise(FSize, TColorFormat.RGB32F);

			FCurrent.Initialise(FSize, TColorFormat.L8);
			FPrevious.Initialise(FSize, TColorFormat.L8);
			FVelocityX.Initialise(FSize, TColorFormat.L32F);
			FVelocityY.Initialise(FSize, TColorFormat.L32F);
            FFlow.Initialise(FSize, TColorFormat.RGB32F); // must be CV_32FC2, but we have our own colorformat tye here ...
        }

		public override void Process()
		{
			CVImage swap = FPrevious;
			FPrevious = FCurrent;
			FCurrent = swap;

			FInput.Image.GetImage(TColorFormat.L8, FCurrent);

			Image<Gray, byte> p = FPrevious.GetImage() as Image<Gray, byte>;
			Image<Gray, byte> c = FCurrent.GetImage() as Image<Gray, byte>;
			Image<Gray, float> vx = FVelocityX.GetImage() as Image<Gray, float>;
			Image<Gray, float> vy = FVelocityY.GetImage() as Image<Gray, float>;

            
			//OpticalFlow.HS(p, c, UsePrevious, vx, vy, FLambda, new MCvTermCriteria(FIterations));                       
            DualTVL1OpticalFlow of = new DualTVL1OpticalFlow();
            of.Calc(c, p, FFlow.GetImage());

            FOutput.Image.SetImage(FFlow);
            //CopyToRgb();
			FOutput.Send();

		}

		private unsafe void CopyToRgb()
		{
			float* sourcex = (float*) FVelocityX.Data.ToPointer();
			float* sourcey = (float*) FVelocityY.Data.ToPointer();
			float* dest = (float*) FOutput.Image.Data.ToPointer();

			for (int i = 0; i < FSize.Width * FSize.Height; i++)
			{
				*dest++ = *sourcex++;
				*dest++ = *sourcey++;
				*dest++ = 0.0f;
			}
		}
	}
}
