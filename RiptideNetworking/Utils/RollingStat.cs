// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

namespace Riptide.Utils
{
    /// <summary>Represents a rolling series of numbers.</summary>
    public class RollingStat
    {
        /// <summary>The position in the array of the latest item.</summary>
        private int _index;
        /// <summary>How many of the array's slots are in use.</summary>
        private int _slotsFilled;
        /// <inheritdoc cref="Mean"/>
        private double _mean;
        /// <summary>The sum of the mean subtracted from each value in the array.</summary>
        private double _sumOfSquares;
        /// <summary>The array used to store the values.</summary>
        private readonly double[] _array;

        /// <summary>The mean of the stat's values.</summary>
        public double Mean => _mean;
        /// <summary>The variance of the stat's values.</summary>
        public double Variance => _slotsFilled > 1 ? _sumOfSquares / (_slotsFilled - 1) : 0;
        /// <summary>The standard deviation of the stat's values.</summary>
        public double StandardDev
        {
            get
            {
                double variance = Variance;
                if (variance >= double.Epsilon)
                {
                    double root = Math.Sqrt(variance);
                    return double.IsNaN(root) ? 0 : root;
                }

                return 0;
            }
        }

        /// <summary>Initializes the stat.</summary>
        /// <param name="sampleSize">The number of values to store.</param>
        public RollingStat(int sampleSize)
        {
            _index = 0;
            _slotsFilled = 0;
            _mean = 0;
            _sumOfSquares = 0;
            _array = new double[sampleSize];
        }
        
        /// <summary>Adds a new value to the stat.</summary>
        /// <param name="value">The value to add.</param>
        public void Add(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return;

            _index %= _array.Length;
            double oldMean = _mean;
            double oldValue = _array[_index];
            _array[_index] = value;
            _index++;

            if (_slotsFilled == _array.Length)
            {
                double delta = value - oldValue;
                _mean += delta / _slotsFilled;
                _sumOfSquares += delta * (value - _mean + (oldValue - oldMean));
            }
            else
            {
                _slotsFilled++;
                double delta = value - oldMean;
                _mean += delta / _slotsFilled;
                _sumOfSquares += delta * (value - _mean);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_slotsFilled == _array.Length)
                return string.Join(",", _array);

            return string.Join(",", _array.Take(_slotsFilled));
        }
    }
}
