// A small Three.js viewer: scene, camera, orbit controls, lights, and a
// content slot that disposes what it replaces and refits the camera.

import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { disposeObject3D } from '../adapters/three.js';

export class Viewer {
    readonly scene = new THREE.Scene();
    readonly camera: THREE.PerspectiveCamera;
    readonly renderer: THREE.WebGLRenderer;
    readonly controls: OrbitControls;
    private content: THREE.Object3D | null = null;

    constructor(container: HTMLElement) {
        this.scene.background = new THREE.Color(0x0f1115);

        this.camera = new THREE.PerspectiveCamera(50, 1, 0.01, 100);
        this.camera.position.set(3, 2, 4);

        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setPixelRatio(window.devicePixelRatio);
        container.appendChild(this.renderer.domElement);

        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;

        this.scene.add(new THREE.HemisphereLight(0xcfd8e6, 0x30363f, 1.1));
        const key = new THREE.DirectionalLight(0xffffff, 1.6);
        key.position.set(4, 6, 3);
        this.scene.add(key);
        const grid = new THREE.GridHelper(10, 20, 0x2a3140, 0x1c212b);
        grid.position.y = -1.6;
        this.scene.add(grid);

        const resize = () => {
            const w = container.clientWidth, h = container.clientHeight;
            if (w === 0 || h === 0) return;
            this.camera.aspect = w / h;
            this.camera.updateProjectionMatrix();
            this.renderer.setSize(w, h);
        };
        new ResizeObserver(resize).observe(container);
        resize();

        this.renderer.setAnimationLoop(() => {
            this.controls.update();
            this.renderer.render(this.scene, this.camera);
        });
    }

    setContent(object: THREE.Object3D): void {
        if (this.content) {
            this.scene.remove(this.content);
            disposeObject3D(this.content);
        }
        this.content = object;
        this.scene.add(object);
        this.fitCamera(object);
    }

    private fitCamera(object: THREE.Object3D): void {
        const box = new THREE.Box3().setFromObject(object);
        const sphere = box.getBoundingSphere(new THREE.Sphere());
        const distance = (sphere.radius / Math.tan((this.camera.fov * Math.PI) / 360)) * 1.25;
        const direction = new THREE.Vector3(0.55, 0.45, 1).normalize();
        this.camera.position.copy(sphere.center.clone().add(direction.multiplyScalar(distance)));
        this.controls.target.copy(sphere.center);
        this.controls.update();
    }
}
