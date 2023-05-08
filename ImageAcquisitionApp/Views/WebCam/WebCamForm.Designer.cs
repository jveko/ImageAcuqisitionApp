namespace ImageAcquisitionApp.Views.WebCam
{
  partial class WebCamForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.cameraLabel = new System.Windows.Forms.Label();
      this.framePerSecondLabel = new System.Windows.Forms.Label();
      this.forceCaptureButton = new System.Windows.Forms.Button();
      this.captureStatusLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(12, 40);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(640, 393);
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // timer1
      // 
      this.timer1.Interval = 30;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // cameraLabel
      // 
      this.cameraLabel.Location = new System.Drawing.Point(263, 9);
      this.cameraLabel.Name = "cameraLabel";
      this.cameraLabel.Size = new System.Drawing.Size(100, 23);
      this.cameraLabel.TabIndex = 1;
      this.cameraLabel.Text = "Camera";
      // 
      // framePerSecondLabel
      // 
      this.framePerSecondLabel.Location = new System.Drawing.Point(24, 446);
      this.framePerSecondLabel.Name = "framePerSecondLabel";
      this.framePerSecondLabel.Size = new System.Drawing.Size(100, 23);
      this.framePerSecondLabel.TabIndex = 2;
      this.framePerSecondLabel.Text = "FPS : 0";
      // 
      // forceCaptureButton
      // 
      this.forceCaptureButton.Location = new System.Drawing.Point(567, 446);
      this.forceCaptureButton.Name = "forceCaptureButton";
      this.forceCaptureButton.Size = new System.Drawing.Size(75, 23);
      this.forceCaptureButton.TabIndex = 3;
      this.forceCaptureButton.Text = "Caputre";
      this.forceCaptureButton.UseVisualStyleBackColor = true;
      this.forceCaptureButton.Click += new System.EventHandler(this.forceCaptureButton_Click);
      // 
      // captureStatusLabel
      // 
      this.captureStatusLabel.Location = new System.Drawing.Point(24, 469);
      this.captureStatusLabel.Name = "captureStatusLabel";
      this.captureStatusLabel.Size = new System.Drawing.Size(191, 23);
      this.captureStatusLabel.TabIndex = 4;
      this.captureStatusLabel.Text = "Status Capture";
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(664, 504);
      this.Controls.Add(this.captureStatusLabel);
      this.Controls.Add(this.forceCaptureButton);
      this.Controls.Add(this.framePerSecondLabel);
      this.Controls.Add(this.cameraLabel);
      this.Controls.Add(this.pictureBox1);
      this.Name = "WebCamForm";
      this.Text = "Webcam Test";
      ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
    }

    private System.Windows.Forms.Label captureStatusLabel;

    private System.Windows.Forms.Button forceCaptureButton;

    private System.Windows.Forms.Label cameraLabel;
    private System.Windows.Forms.Label framePerSecondLabel;

    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Timer timer1;

    #endregion
  }
}