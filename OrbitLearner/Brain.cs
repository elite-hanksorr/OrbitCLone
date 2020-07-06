using System;
using System.Collections.Generic;

using static Tensorflow.Binding;
using Tensorflow;
using NumSharp;

namespace OrbitLearner
{
    public class Brain
    {
        public Tensor inputs;
        public Tensor model;

        //shape input describes the shape of the nn
        //e.g. {5, 3, 2, 1} means a nn with 5 inputs, 3 and 2 neuron hidden layers, and 1 output
        public Brain(int[] shape)
        {
            if (shape.Length == 0)
                throw new ArgumentOutOfRangeException("layers cannot be empty");

            inputs = tf.placeholder(tf.float32, shape[0]);
            var x = inputs;

            foreach (var n in shape) {
                var W = tf.get_variable("W", shape: new int[]{n, x.dims[0]}, initializer: tf.glorot_uniform_initializer);
                var b = tf.get_variable("b", shape: n, initializer: tf.zeros_initializer);
                x = W * x + b;
            }

            model = x;
        }

        public NDArray Eval(NDArray inputs)
        {
            using (var sess = tf.Session())
            {
                var result = sess.run(model, feed_dict: new FeedItem[]
                {
                    new FeedItem(this.inputs, inputs)
                });

                return result;
            }
        }
    }
}
