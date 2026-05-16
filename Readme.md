# Abstract Reasoning Process

<p align="center">
    <img src="./Doc/Results.gif" width = "400">
    <p align="center">
        <em>Baseball trajectory tracking result (240 fps reconstruction)</em>
    </p>
</p>

---

## Target
measure the speed vector of the baseball

## Given Parameters

| Parameter | Value | Note |
|---|---|---|
| Pixel Size | $0.0048\,\text{mm} = 4.8 \times 10^{-6}\,\text{m}$ |  |
| Full Resolution | $1024W \times 1280H$ | Original file defined as 1280 × 1024 |
| Frame Rate | $240\,\text{fps}$ |  |
| Focal Length | $8\,\text{mm} = 0.008\,\text{m}$ |  |
| Baseball Radius | $0.0373\,\text{m}$ |  |
| Camera Pitch | $10^\circ$ upward tilt |  |
| Camera Position | Ground level, approximately 4 meters from ball launch position |  |
| Pinhole Model Center | $(c_x, c_y) = (512, 640)$ |  |

## Assumptions 

- The camera is placed directly in front of the batter, facing the batter.
- The camera height is assumed to be at ground level, aligned with the batter’s feet.
- Therefore, yaw = 0° and roll = 0°.
- \(t = 0\) is defined as IMG04, corresponding to the moment immediately after the ball is hit.
- A pinhole camera model is used with the principal point at \((c_x, c_y) = (512, 640)\).

	
## Process:

### Detection Results

The baseball was detected in each frame using the Hough Circle Transform. The detection results are listed below:

| Frame | Point X | Point Y | Radius |
|---|---:|---:|---:|
| IMG01.bmp | - | - | - |
| IMG02.bmp | - | - | - |
| IMG03.bmp | - | - | - |
| IMG04.bmp | 511 | 799 | 13 |
| IMG05.bmp | 481 | 792 | 14 |
| IMG06.bmp | - | - | - |
| IMG07.bmp | 417 | 778 | 16 |
| IMG08.bmp | 378 | 771 | 16 |
| IMG09.bmp | 340 | 760 | 19 |
| IMG10.bmp | - | - | - |
| IMG11.bmp | - | - | - |
| IMG12.bmp | 203 | 732 | 19 |
| IMG13.bmp | 149 | 719 | 19 |
| IMG14.bmp | 93 | 707 | 22 |
| IMG15.bmp | 28 | 697 | 22 |


### Linear Interpolation

Due to inconsistent ball motion direction and increased detection difficulty in the first three frames, IMG01–IMG03 are excluded from further analysis.

For the remaining frames (IMG04–IMG15), missing values are filled using linear interpolation to ensure temporal continuity of the trajectory.

| Frame | Point X | Point Y | Radius |
|---|---:|---:|---:|
| IMG04.bmp | 511 | 799 | 13 |
| IMG05.bmp | 481 | 792 | 14 |
| IMG06.bmp | 449 | 785 | 15 |
| IMG07.bmp | 417 | 778 | 16 |
| IMG08.bmp | 378 | 771 | 16 |
| IMG09.bmp | 340 | 760 | 19 |
| IMG10.bmp | 294 | 751 | 19 |
| IMG11.bmp | 248 | 742 | 19 |
| IMG12.bmp | 203 | 732 | 19 |
| IMG13.bmp | 149 | 719 | 19 |
| IMG14.bmp | 93  | 707 | 22 |
| IMG15.bmp | 28  | 697 | 22 |

### Depth (Z) Derivation

The depth is estimated using the pinhole camera model based on the observed ball radius in image space.

$$ Z = \frac{f \cdot R}{r_{px} \cdot s} $$

This can be simplified as:

$$ Z = \frac{62.1666}{r_{px}} $$

where:
- $f$: focal length of the camera (m) = 0.008m
- $R$: real radius of the baseball (m) = 0.0373m
- $s$: physical pixel size (m/pixel) = $4.8 \times 10^{-6}$m
- $r_{px}$: detected radius in image space (pixels)  

Therefore, the depth \(Z\) can be computed directly from the detected radius.

| Frame | Point X | Point Y | Radius (px) | Depth Z (m) |
|---|---:|---:|---:|---:|
| IMG04.bmp | 511 | 799 | 13 | 4.782 |
| IMG05.bmp | 481 | 792 | 14 | 4.440 |
| IMG06.bmp | 449 | 785 | 15 | 4.144 |
| IMG07.bmp | 417 | 778 | 16 | 3.885 |
| IMG08.bmp | 378 | 771 | 16 | 3.885 |
| IMG09.bmp | 340 | 760 | 19 | 3.272 |
| IMG10.bmp | 294 | 751 | 19 | 3.272 |
| IMG11.bmp | 248 | 742 | 19 | 3.272 |
| IMG12.bmp | 203 | 732 | 19 | 3.272 |
| IMG13.bmp | 149 | 719 | 19 | 3.272 |
| IMG14.bmp | 93  | 707 | 22 | 2.826 |
| IMG15.bmp | 28  | 697 | 22 | 2.826 |

---

### 3D Reconstruction from 2D Image Coordinates using Depth Z

The depth is estimated from the observed radius in pixel space:

$$ Z = \frac{f \cdot R}{r_{px} \cdot s} $$

where:
- $f$: focal length (m)  
- $R$: real baseball radius (m)  
- $r_{px}$: detected radius in pixels  
- $s$: pixel size (m)

---

### Pinhole Projection Model

Depth-based 3D reconstruction equations:

$$ x = (u - c_x)\cdot s, \quad y = (v - c_y)\cdot s $$

$$ X = \frac{Zx}{f}, \quad Y = \frac{Zy}{f} $$

Combined form:

$$ X = \frac{Z (u - c_x)\cdot s}{f}, \quad Y = \frac{Z (v - c_y)\cdot s}{f} $$

---

### Variable Definitions

- $u$: pixel x-coordinate  
- $v$: pixel y-coordinate  
- $c_x, c_y$: principal point (pinhole center)  
- $Z$: depth (m)  
- $s$: pixel size = $4.8 \times 10^{-6}$ m  
- $f$: focal length (m)  

---

### Reconstructed 3D Coordinates (World Frame)

| Frame | X (m) | Y (m) | Z (m) |
|------|------:|------:|------:|
| IMG04 | -0.0028692 | 0.4562028 | 4.782 |
| IMG05 | -0.0825840 | 0.4049280 | 4.440 |
| IMG06 | -0.1566432 | 0.3605280 | 4.144 |
| IMG07 | -0.2214450 | 0.3216780 | 3.885 |
| IMG08 | -0.3123540 | 0.3053610 | 3.885 |
| IMG09 | -0.3376704 | 0.2355840 | 3.272 |
| IMG10 | -0.4279776 | 0.2179152 | 3.272 |
| IMG11 | -0.5182848 | 0.2002464 | 3.272 |
| IMG12 | -0.6066288 | 0.1806144 | 3.272 |
| IMG13 | -0.7126416 | 0.1550928 | 3.272 |
| IMG14 | -0.7104564 | 0.1136052 | 2.826 |
| IMG15 | -0.8206704 | 0.0966492 | 2.826 |

---

### Time Difference

$$ dt = \frac{11}{240} = 0.0458333333 \, \text{s} $$

$$ \frac{1}{dt} = \frac{240}{11} = 21.8181818 $$

---

### Camera Velocity (Unrotated)

The velocity is computed by taking the difference between two 3D positions:

$$ \Delta P = P_2 - P_1 $$

---

### Position Difference

$$ \Delta x = -0.8206704 - (-0.0028692) = -0.8178012 $$
$$ \Delta y = 0.0966492 - 0.4562028 = -0.3595536 $$
$$ \Delta z = 2.8260 - 4.7820 = -1.9560 $$

---

### Velocity in Camera Coordinates

$$ V_{cam} = \frac{\Delta P}{dt} = \Delta P \cdot 21.8181818 $$

$$ V_x = -0.8178012 \times 21.81818 = -17.84 \, \text{m/s} $$
$$ V_y = -0.3595536 \times 21.81818 = -7.85 \, \text{m/s} $$
$$ V_z = -1.9560 \times 21.81818 = -42.68 \, \text{m/s} $$

---

### Final Camera Velocity

$$ V_{cam} = (-17.84,\ -7.85,\ -42.68)\ \text{m/s} $$

---

## Velocity Rotation (10° Pitch About X-Axis)

The camera pitch is corrected by rotating the velocity vector only (not the position), using a rotation around the X-axis.

---

### Rotation Parameters

$$ \theta = 10^\circ $$

$$ \cos\theta = 0.98480775, \quad \sin\theta = 0.17364818 $$

---

### Rotation Matrix (X-axis)

$$ V_x' = V_x $$

$$ V_y' = V_y \cos\theta - V_z \sin\theta $$

$$ V_z' = V_y \sin\theta + V_z \cos\theta $$

---

### Final World Frame Velocity

$$ V_{world} = (-17.84,\ -0.31,\ -43.39)\ \text{m/s} $$

---

### Speed Magnitude

$$ |V| = \sqrt{(-17.84)^2 + (-0.31)^2 + (-43.39)^2} $$

$$ |V| \approx 46.92 \, \text{m/s} $$

---
