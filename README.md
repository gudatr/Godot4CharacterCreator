# Godot 4 Character Creator

![image](https://github.com/gudatr/Godot4CharacterCreator/blob/main/Avatar.png?raw=true)

### Introduction

A 3D character creator for Godot 4.2+ based on lexpartizan/Go_MakeHuman_dot

The system is currently capable of all the things the original Godot 3.X system is, adds several features on top and is generally a lot faster when morphing.
For upcoming features and future development see the following roadmap.

### Initial setup & guidance

Please consult the repository's wiki for further information.

If you have any questions, want to dicuss something or have an idea how to improve the project, please check out the dicussions page.

### Roadmap

#### 1. Expression Player

- [x] Automatic Blinking
- [x] Random head and eye movements
- [x] Look At function
- [x] Morph expressions dynamically
- [x] Lip sync (Basic, jaw and lips movement)
- [x] Automatic skinning and blendshape transfer for cloth meshes
- [x] Saving
- [x] Complete C# API

#### 2. Overlays

- [x] Overlays implementation for body
- [x] Automatic atlas texture generation
- [x] Overlays for clothing
- [x] Saving
- [x] Complete C# API

#### 3. Better Default UI

- [x] Blendshape sliders
- [x] Sex, Age and Ethnicity
- [x] Overlays
- [x] Wardrobe
- [x] Proper Scaling

#### 4. Performance optimizations & Refactoring

- [x] Speed up mesh_generation workflow (almost fully utilizes CPU)
- [x] Move as much of the workload as is possible to the thread pool
- [x] Hide parts of the body dynamically so they don't bleed
- [x] Separate blend shape data into files so it can be loaded and unloaded dynamically to reduce memory usage
- [x] Standardize formatting for resources & and Code
- [ ] Create a benchmark scenario for characters and optimize

#### 5. Documentation

- [x] Code Documentation for functionality
- [ ] Github Wiki for functionality & setup
- [ ] Intro Video, showing off the creation example
- [ ] Remakes of the videos from the old repo

#### 6. Tutorials

- [ ] Cloth creation
- [ ] Animations

#### 7. Content

- [ ] Include MakeHuman asset packs

#### Contributions

If you contribute to the project, please specify what license applies to your assets at the time of committing, otherwise you agree for them to be published under CC0.
No licenses more restrictive than CC-BY 4.0 will be accepted.
Contributions regarding new cloth and skins and improvements of existing content are greatly appreciated.

CC-BY 4.0 https://creativecommons.org/licenses/by/4.0/
CC-BY 1.0 https://creativecommons.org/licenses/by/1.0/

#### Licensing

Since a lot of the content used within the project originates from MakeHuman or its community, the MakeHuman license text was added as a reference.
This project does not use any code from the MakeHuman codebase which falls under the GNU Affero General Public License.
It only uses assets licensed under CC0 by the original MakeHuman Team / Contributors or specifies which licenses apply for the asset individually.
The project itself is licensed under MIT.
Where applicable, additional license information is provided.

##### All these licenses allow at least closed source usage with credits and license visible in your product.


