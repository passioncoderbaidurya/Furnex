using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32; // Added for registry access

namespace Furnex
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBox1.Text.Trim(), out double mV) &&
                double.TryParse(textBox2.Text.Trim(), out double lm35_temp))
            {
                // Call the function with double values
                finalize(mV, lm35_temp);
            }
            else
            {
                MessageBox.Show("Please enter valid data.");
            }
        }

        private bool IsValidDouble(string input)
        {
            // Check if input is a valid double
            return double.TryParse(input.Trim(), out _);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = IsValidDouble(textBox1.Text) && IsValidDouble(textBox2.Text);
        }

        private void finalize(double mV, double lm35_temp)
        {
            if (mV >= 21)
            {
                MessageBox.Show("Millivolts must be less than 21.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (mV <= 10 && mV > 0)
            {
                MessageBox.Show("Millivolts value too low.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (mV < 0 && lm35_temp < 0)
            {
                MessageBox.Show("Millivolts and ambient values are INVALID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (mV < 0 && lm35_temp > 0)
            {
                MessageBox.Show("Millivolts value INVALID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (mV > 0 && lm35_temp < 0)
            {
                MessageBox.Show("Ambient value INVALID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (lm35_temp > 60)
            {
                MessageBox.Show("Ambient temperature must be less than or equal to 60.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                double error = CalcError(lm35_temp);
                double sensitivity = CalculateSensitivity(mV);
                double compensated_mV = mV + lm35_temp * sensitivity;
                double final_temp = Math.Round(CalcTemp(compensated_mV) - error);
                Form2 form2 = new Form2();
                form2.NumericValue = final_temp;
                form2.Show();
            }
        }

        static double CalcTemp(double x)
        {
            double mean = 17.08;
            double std_dev = 2.519;
            double x_norm = (x - mean) / std_dev;
            double p1 = 0.0279;
            double p2 = 0.0803;
            double p3 = 0.0839;
            double p4 = -0.1272;
            double p5 = -0.3486;
            double p6 = 0.0553;
            double p7 = 1.1046;
            double p8 = 1.0532;
            double p9 = 178.6836;
            double p10 = 1473.9;
            double result = p1 * Math.Pow(x_norm, 9) + p2 * Math.Pow(x_norm, 8) + p3 * Math.Pow(x_norm, 7) +
                            p4 * Math.Pow(x_norm, 6) + p5 * Math.Pow(x_norm, 5) + p6 * Math.Pow(x_norm, 4) +
                            p7 * Math.Pow(x_norm, 3) + p8 * Math.Pow(x_norm, 2) + p9 * x_norm + p10;
            return result;
        }

        static double CalculateSensitivity(double x)
        {
            // Constants
            const double mean = 17.08;
            const double std_dev = 2.519;
            const double p1 = 0.0279;
            const double p2 = 0.0803;
            const double p3 = 0.0839;
            const double p4 = -0.1272;
            const double p5 = -0.3486;
            const double p6 = 0.0553;
            const double p7 = 1.1046;
            const double p8 = 1.0532;
            const double p9 = 178.6836;

            // Normalize x (mV to normalized units)
            double x_norm = (x - mean) / std_dev;

            // Calculate dT/dx_norm - derivative of the polynomial with respect to x_norm
            double dT_dx_norm = 9 * p1 * Math.Pow(x_norm, 8) +
                               8 * p2 * Math.Pow(x_norm, 7) +
                               7 * p3 * Math.Pow(x_norm, 6) +
                               6 * p4 * Math.Pow(x_norm, 5) +
                               5 * p5 * Math.Pow(x_norm, 4) +
                               4 * p6 * Math.Pow(x_norm, 3) +
                               3 * p7 * Math.Pow(x_norm, 2) +
                               2 * p8 * x_norm +
                               p9;

            // Sensitivity in °C/mV, now convert to mV/°C
            double sensitivity_C_per_mV = dT_dx_norm / std_dev;

            // Return sensitivity in mV/°C
            return 1.0 / sensitivity_C_per_mV;
        }

        static double CalcError(double x)
        {
            const double mean = 31.93;
            const double std = 16.73;
            const double p1 = 0.0837, p2 = -0.0504, p3 = -0.6865, p4 = 0.4057, p5 = 1.9238,
                         p6 = -1.0722, p7 = -2.1327, p8 = 0.8552, p9 = 10.2698, p10 = 18.8668;

            // Normalize x
            double normalizedX = (x - mean) / std;

            // Evaluate the polynomial
            return p1 * Math.Pow(normalizedX, 9) +
                   p2 * Math.Pow(normalizedX, 8) +
                   p3 * Math.Pow(normalizedX, 7) +
                   p4 * Math.Pow(normalizedX, 6) +
                   p5 * Math.Pow(normalizedX, 5) +
                   p6 * Math.Pow(normalizedX, 4) +
                   p7 * Math.Pow(normalizedX, 3) +
                   p8 * Math.Pow(normalizedX, 2) +
                   p9 * normalizedX +
                   p10;
        }

        private void label3_Click_1(object sender, EventArgs e)
        {
        }
    }
}