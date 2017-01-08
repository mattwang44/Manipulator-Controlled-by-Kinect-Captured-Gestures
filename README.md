The code is modified from "Skeletal Viewer", the sample code of Microsoft Kinect for Xbox.

# Manipulator Controlled by Gestures with Kinect
Term Project of [Introduction to Robotics 2016](https://nol.ntu.edu.tw/nol/coursesearch/print_table.php?course_id=522%20U1290&class=&dpt_code=5220&ser_no=50327&semester=105-1&lang=EN) in Department of Mechanical Engineering, National Taiwan University (NTUME).

The manipulator imitates the motions of a captured right arm and clenches when the raising of captured left arm is detected.  

With the function of "Skeletal Tracking" of KinectSDK, the positions of 20 body joints can be determined. The joint angles and the angles of servo motors are computed in the computer and the the command is send to the Arduino board via serial port. Via [Firmata](http://www.firmata.org/wiki/Main_Page) protocol, the commands of Arduino controlling the servo motors can be programmed in C# without modifying the arduino code. 

## Author
Wei-hsiang Wang

Collaborator: Ho-tai Tsai, Chin-an Pao, Wei-tun Chan

Grad. students of [Intellectual Property Analysis & Innovative Design Laboratory](http://iaid.me.ntu.edu.tw/), NTUME

## Setup
1. [KinectSDK64.msi](https://github.com/atosorigin/Kinect/blob/master/lib/Third%20Party/Microsoft%20Kinect%20SDK/KinectSDK64.msi)

2. [Coding4Fun Kinect Toolkit](https://c4fkinect.codeplex.com/releases/view/68333)

## Programming Environment
Visual Studio C# in Win10

## Hardware
1. Kinect for Xbox (1414)

2. Arduino Uno ( Uploaded with the sample sketchbook "ServoFirmata" in Arduino IDE )

3. Self-made Manipulator (LEGO)

4. Servo Motor *4
![](/image/1.jpg) 

# Interface Preview
Images captured by Kinect and positions of joints are displayed.
![](/image/3.gif) 

# Reference
1. [Kinect Programming Guide](http://research.microsoft.com/en-us/um/redmond/projects/kinectsdk/docs/ProgrammingGuide_KinectSDK.pdf)

2. [MSDN Taiwan](https://www.youtube.com/user/TWDevelopGirl/search?query=kinect)

3. [C# Programming Guide by MSDN](https://msdn.microsoft.com/en-us/library/67ef8sbd.aspx)

4. [Head First C#](http://shop.oreilly.com/product/0636920027812.do)
