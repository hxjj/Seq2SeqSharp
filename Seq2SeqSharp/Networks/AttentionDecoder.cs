﻿
using Seq2SeqSharp.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TensorSharp;

namespace Seq2SeqSharp
{


    [Serializable]
    public class AttentionDecoder : INeuralUnit
    {
        List<LSTMAttentionDecoderCell> m_decoders = new List<LSTMAttentionDecoderCell>();
        int m_hdim;
        int m_dim;
        int m_depth;
        int m_context;
        int m_deviceId;
        AttentionUnit m_attentionLayer;
        string m_name;

        public AttentionDecoder(string name, int hiddenDim, int embeddingDim, int contextDim, int depth, int deviceId)
        {
            m_attentionLayer = new AttentionUnit($"{name}.AttnUnit", hiddenDim, contextDim, deviceId);

            m_name = name;
            m_hdim = hiddenDim;
            m_dim = embeddingDim;
            m_context = contextDim;
            m_depth = depth;
            m_deviceId = deviceId;

            m_decoders.Add(new LSTMAttentionDecoderCell($"{name}.LSTMAttn_0", hiddenDim, embeddingDim, contextDim, deviceId));
            for (int i = 1; i < depth; i++)
            {
                m_decoders.Add(new LSTMAttentionDecoderCell($"{name}.LSTMAttn_{i}", hiddenDim, hiddenDim, contextDim, deviceId));
            }
        }

        public int GetDeviceId()
        {
            return m_deviceId;
        }

        public INeuralUnit CloneToDeviceAt(int deviceId)
        {
            return new AttentionDecoder(m_name, m_hdim, m_dim, m_context, m_depth, deviceId);
        }


        public void Reset(IWeightFactory weightFactory, int batchSize)
        {
            foreach (var item in m_decoders)
            {
                item.Reset(weightFactory, batchSize);
            }

        }

        public AttentionPreProcessResult PreProcess(IWeightTensor encoderOutput, int batchSize, IComputeGraph g)
        {
            return m_attentionLayer.PreProcess(encoderOutput, batchSize, g);
        }


        public IWeightTensor Decode(IWeightTensor input, AttentionPreProcessResult attenPreProcessResult, int batchSize, IComputeGraph g)
        {
            var V = input;
            var lastStatus = this.m_decoders.LastOrDefault().Cell;
            var context = m_attentionLayer.Perform(lastStatus, attenPreProcessResult, batchSize, g);

            foreach (var decoder in m_decoders)
            {
                var e = decoder.Step(context, V, g);
                V = e;
            }

            return V;
        }

        public List<IWeightTensor> GetCTs()
        {
            List<IWeightTensor> res = new List<IWeightTensor>();
            foreach (var decoder in m_decoders)
            {
                res.Add(decoder.Cell);
            }

            return res;
        }

        public List<IWeightTensor> GetHTs()
        {
            List<IWeightTensor> res = new List<IWeightTensor>();
            foreach (var decoder in m_decoders)
            {
                res.Add(decoder.Hidden);
            }

            return res;
        }

        public void SetCTs(List<IWeightTensor> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                m_decoders[i].Cell = l[i];
            }
        }

        public void SetHTs(List<IWeightTensor> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                m_decoders[i].Hidden = l[i];
            }
        }

        public List<IWeightTensor> GetParams()
        {
            List<IWeightTensor> response = new List<IWeightTensor>();

            foreach (var item in m_decoders)
            {
                response.AddRange(item.getParams());
            }
            response.AddRange(m_attentionLayer.GetParams());

            return response;
        }

        public void Save(Stream stream)
        {
            m_attentionLayer.Save(stream);
            foreach (var item in m_decoders)
            {
                item.Save(stream);
            }
        }

        public void Load(Stream stream)
        {
            m_attentionLayer.Load(stream);
            foreach (var item in m_decoders)
            {
                item.Load(stream);
            }
        }
    }
}
