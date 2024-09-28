MULTI WAVELENGTH GALAXY ILLUSTRATOR (MGI) - USER GUIDE
Introduction
The Multi Wavelength Galaxy Illustrator (MGI) is a data visualization software framework designed for visualizing astronomical FITS files, with a particular focus on galaxy representation. MGI facilitates the management and immersive visualization of multi-wavelength astronomical datasets in both desktop and immersive environments. The application focuses on astronomical data and the analytical tools are tailored to the domain, however the MGI can be modified to provide substantial benefit to other disciplines.  It is developed using C# and integrated with game engine technologies, MGI offers unique tools and workflows for interacting with vast astronomical datasets. The framework supports three workflows and has three different versions: a desktop application (MGI-Desktop), a virtual reality (VR) application (MGI-VR), and an augmented reality (AR) implementation at the University of Portsmouth (MGI-AR).

Software Overview
The specific goal of MGI is to enable the scientific representation and interrogation of 2D FITS file in a 3D like environment. MGI represents a pioneering approach to visualizing FITS files and offers tools capable of managing complex multi-wavelength datasets in immersive environments. The MGI has been developed  in collaboration with Researchers at INAF -Italy. For more details about the MGI software development and all its aspects , please contact David Amuneni on osuhamuneni@gmail.com .
The significance of MGI lies in its ability to provide a conducive environment for visualizing next-generation astronomical data, particularly for the scientific community. Among the suggested tools are features for immersive exploration, modification, and interaction with astronomical datasets.

Code Base and Release
Suggested Tools:
- FITS File Visualization Interface: A tool for visualizing FITS files stored in data archives like the Sloan Digital Sky Survey (SDSS), allowing for immersive viewing of stellar objects, galaxies, and cosmic structures.
- Image Manipulation: Tools to manipulate image attributes (e.g., color mapping of specific features like density or galaxy age).
- Interactive Tools: Real-time interactive tools for seamless navigation and data exploration, scalable to accommodate a large number of galaxies.
- User-Generated Content: Features for adding user-generated content such as descriptive text or auditory enhancements.
- FITS File Modification: Tools for modifying FITS files and saving the changes for future use.
Code Base and Software Versions
Code Base
The MGI is an open-source project. Its source code is available on the projects Github repository.  Installation guides are provided in the section Installation and configuration, including the installation of the required VR environment.
To build MGI from source, please see the Building MGI from source link.
1. Desktop Version
The MGI desktop version provides a fully interactive interface for users to visualize and explore FITS files. This version is ideal for those who want a user-friendly platform to access and manipulate multi-wavelength data in a 2D or 3D space.
2. Virtual Reality (VR) Application
The VR version immerses users in a 3D galaxy environment using VR headsets such as the Oculus Quest, HTC Vive, or Valve Index. This version supports real-time interaction, where users can walk through galaxies and observe astronomical data with full immersion.
VR Specifications:
- Frame rate of at least 90Hz is required for smooth performance.
- The prototype achieved a frame rate of 120Hz consistently, ensuring minimal latency and a smooth user experience.
- Uses OpenXR, a universal API standard that bridges VR and AR platforms.
3. Augmented Reality (AR) Implementation
MGI’s AR version is deployed at the University of Portsmouth’s AR laboratory, where real-world interactions are combined with galaxy data visualizations. This implementation is ideal for educational presentations and immersive research environments.
System Requirements
Desktop Version:
OS: Windows 10 or later
CPU: Intel i5 or equivalent
RAM: 8 GB minimum
Graphics: NVIDIA GTX 1050 or higher
Storage: 5 GB of free space
Virtual Reality (VR) Application:
Any VR headset compatible with SteamVR should function. The following VR headsets are recommended (tested):VR Headset: 
•	Oculus Rift S
•	Meta Quest 2
•	 HTC Vive
•	HTC Vive Pro
•	Valve Index
Note : All of these headsets should work, but you might have to change the control bindings in the SteamVR or VR interface.
OS: Windows 10 or later
CPU: Intel i7 or equivalent
RAM: 16 GB
Graphics: NVIDIA GTX 1070 or higher
Storage: 10 GB of free space
Augmented Reality (AR) Implementation:
AR Hardware: Microsoft HoloLens or compatible device
OS: Windows 10 Holographic
Note: Generally, a quad-core or higher CPU is recommended. At least 16 GB of RAM is required for steady visualization. However, the size of the FITS file usable will depend heavily on system memory capacity. In all a 32 GB, or 64 GB is recommended to support large FITS files in memory. Also , the latest 64-bit (X64) Visual C++ redistributable needs to be installed.
Installation and Configuration

The software is in active development and efforts have been made to ensure that the current version has no issues bug reports and feature request are welcome on the Github repository.


Executable

MGI Workflows

Workflow 1: Basic Visualization
Designed for users who want to explore astronomical data quickly and easily. With a focus on straightforward visualization, this workflow allows users to load and view FITS files with minimal customization.
Workflow 2: Prototyping of Visualizations
This workflow enables users to experiment with visualizations, adjusting various parameters such as color mapping, scale, and visual effects. Ideal for researchers and advanced users looking to prototype custom visualizations.
Workflow 3: Repurposing the MGI Framework
For developers and researchers who want to extend MGI’s functionality or create entirely new tools, this workflow provides full access to the MGI framework. Users can modify the C# code to build custom tools or integrate external datasets.
Immersive Visualizations
Prototype 1: Desktop Visualization
The first prototype was built around a simple galaxy visualization using the Unity game engine. By leveraging the CSharpFITS DLL, the software can load FITS files into Unity’s graphics pipeline for rendering.
Prototype 2: Virtual Reality Implementation
The second prototype integrates a VR experience, allowing users to navigate an immersive 3D galaxy environment. 
Prototype 3: Scaling Up and Data Integration
The third prototype focuses on data input and scaling, handling large datasets and galaxy archives. 
Implementation Guide
For Basic Users:
1. Once the requirements described in System requirements, are installed and working correctly, the user can download and unzip the provided iDaVIE.1.0.zip, which contains the executable .exe (and other reference files). The zip file is available on Github at the link below:
•	For MGI-Desktop, the link is : 
•	For MGI-VR, the link is 
•	The MGI-AR does not have not have the built version as it is built specifically for the CCIXR lab at the University of Portsmouth. However, the basic visualizer has been created that can be adopted by other AR display screen. The link to the basic visualizer is at  (https://github.com/osuhamuneni/BASIC-VISUALISER)
2. Download the built version: Go to the official MGI GitHub repository(https://github.com/osuhamuneni/BASIC-VISUALISER) and download the binary installation files. 
2. Run the Installer: Follow the on-screen prompts to install the software on your computer.
3. Launch the Application: Once installed, launch the desktop version of the software, and start exploring astronomical FITS files using the basic visualization tools.
Note : The application has been built and tested only for Windows OS. 
For Advanced Users:
1. Download the Raw Files: Access the raw files from the MGI GitHub repository (https://github.com/osuhamuneni/MGI-DESKTOP-VR).
2. Set Up a Development Environment: Open the downloaded raw files in the Unity game engine. Ensure you have the latest version of Unity installed to support the project.
3. Modify the Code: Use the Unity editor and C# scripting environment to make changes to the software’s core functionality.
4. Compile and Test: After making the necessary changes, compile the project within Unity, and test the new features in your development environment.
5. Deploy Your Version: Once satisfied with your modifications, deploy your customized version for either desktop, VR, or AR environments.
The first time MGI is launched, a file named config.json will be generated in the main directory where the executable is located. This file contains the default configuration settings for the software. The user can modify this file to customize the software to their needs. The configuration file has the following settings that can be modified:
Configuration File 

Colourization Mode : Colouration is managed through the application of colour mapping or colour transfer functions, thereby enabling the customization of chromatic attributes.
Spatial Scaling of Galaxies : Spatial scaling of the galaxies within the scene world is mandated to ensure proportional representation. This can be achieved through either a uniform scaling parameter for simplicity or via the utilization of the Petrosian radius as a scaling metric
Cropping of Galaxies : Provide options for cropping out faint stars and celestial bodies at the outer edges of the galaxy.
Denoising of Galaxies : Denoising of galaxies provide options to empower users to prototype specific visualization types for galaxies, providing a means to determine the appearance of galaxies in the scene.

Input and Outputs
In this section we describe what kinds of file can be ingested by the MGI and what files and formats are saved as data products of the MGI.

Inputs
Standard  FITS files from the SDSS archive. 
Outputs
Created or modified masks, or moment maps exported as FITS files.
Conclusion
The Multi Wavelength Galaxy Illustrator (MGI) offers a unique platform for visualizing astronomical data in both desktop and immersive environments. Whether you’re a basic user exploring galaxy datasets or an advanced developer looking to extend the functionality of the software, MGI provides the tools and flexibility needed to transform astronomical data into immersive visual experiences.
For further assistance, visit our support page or contact support@mgi-software.com.
