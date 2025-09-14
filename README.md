# LiveKit XR Streaming Application

A real-time video streaming application that connects mobile browser cameras to Meta Quest VR headsets using LiveKit WebRTC infrastructure. Stream your mobile camera feed directly into VR space with interactive 3D controls.

![Unity Version](https://img.shields.io/badge/Unity-2022.3%20LTS-blue)
![Platform](https://img.shields.io/badge/Platform-Meta%20Quest-green)
![Streaming Service](https://img.shields.io/badge/Streaming%20Service-LiveKit-orange)

## ✨ Features

### 📱 **Mobile to VR Streaming**
- **Real-time video streaming** from mobile browser to Quest headset
- **Audio synchronization** - hear mobile audio through Quest speakers
- **720p30 performance** optimized for smooth VR experience
- **Low latency** streaming via LiveKit

### 🎮 **XR Interactions**
- **Grabbable video screen** - grab and move the video display in 3D space
- **Dynamic scaling** - resize screen using controller thumbstick
- **Picture-in-picture** - toggleable mini screen attached to controller
- **Intuitive controls** - familiar VR interaction patterns

### 🔄 **Connection Management**
- **One-click connection** between mobile and Quest
- **Auto-reconnect** functionality for network drops
- **Real-time status updates** with visual feedback
- **Mute/unmute controls** for audio

## 🛠️ Prerequisites

Before you begin, ensure you have:

### Hardware Requirements
- **Meta Quest 2/3/Pro** VR headset
- **Mobile device** with camera, mic and browser
- **Wi-Fi network** connecting both devices

## 📦 Installation & Setup

### 📱 Download & Install
1. **Download APK:** Go to [Releases](../../releases) and download the latest `android-build.zip` and unzip it.
2. **Enable Developer Mode** on your Meta Quest headset
3. **Install APK** using one of these methods:
   - **SideQuest:** Drag and drop APK into SideQuest
   - **ADB:** `adb install VR_Live_Feed.apk`
   - **Meta Quest Developer Hub:** Use the device manager

### 🎮 Ready to Stream!
4. **Launch app** on Quest headset (find it in "Unknown Sources")
5. **Setup mobile publisher** (instructions below)
6. **Start streaming** your mobile camera to VR!

## 🚀 Usage

### 1. Setup Mobile Publisher
1. **Create LiveKit Room:**

- Go to LiveKit (https://meet.livekit.io/?tab=custom) and create a room
- Save the room name (You'll need to enter this in the game)

2. **Launch app** on Quest headset

3. **Connect to stream:**
- Put on headset
- Enter Web URL
- Enter room name
- Click "Connect to Stream" button
- Wait for "Connected" status

### 3. XR Interaction Controls

**Main Screen Controls:**
- **Grab:** Point controller at screen + hold grip button
- **Move:** While gripping, move controller to reposition screen
- **Scale:** While gripping, use thumbstick up/down to resize
- **Release:** Let go of grip button

**Mini Screen Controls:**
- **Toggle visibility:** Press Trigger on right controller
- **Mini screen** automatically mirrors main screen content

**General Controls:**
- **Mute audio:** Click mute button in UI
- **Disconnect:** Click disconnect button to end session

## 🔧 Configuration

### LiveKit Settings
- **Connection timeout:** Default 30 seconds
- **Auto-reconnect attempts:** Default 3 tries
