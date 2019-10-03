﻿using Seq2SeqSharp.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqSharp
{
    class FeedForwardLayer
    {
        private IWeightTensor m_Whd;
        private IWeightTensor m_Bd;
        private string m_name;

        public FeedForwardLayer(string name, int inputDim, int outputDim, int deviceId)
        {
            m_name = name;
            m_Whd = new WeightTensor(new long[2] { inputDim, outputDim }, deviceId, name: $"{name}.{nameof(m_Whd)}", isTrainable: true);
            m_Bd = new WeightTensor(new long[2] { 1, outputDim }, 0, deviceId, name: $"{name}.{nameof(m_Bd)}", isTrainable: true);
        }

        public IWeightTensor Process(IWeightTensor inputT, IComputeGraph graph)
        {
            var g = graph.CreateSubGraph(m_name);
            return g.Affine(inputT, m_Whd, m_Bd);
        }

        public virtual List<IWeightTensor> GetParams()
        {
            List<IWeightTensor> response = new List<IWeightTensor>();
            response.Add(m_Whd);
            response.Add(m_Bd);

            return response;
        }

        public void Save(Stream stream)
        {
            m_Whd.Save(stream);
            m_Bd.Save(stream);
        }


        public void Load(Stream stream)
        {
            m_Whd.Load(stream);
            m_Bd.Load(stream);
        }
    }
}
