class VisualProgrammer {
    constructor(element, dotNet) {
        this.element = element;
        this.dotNet = dotNet;
        this.onPointerDown = (e) => {
            if (!(e.target instanceof Element))
                return;
            // If the user clicks down on the part of the visual node marked as the dragger, we want to start moving that node.
            if (e.target.classList.contains('vp--node-dragger')) {
                e.preventDefault();
                let offset = VisualProgrammer.getMousePositionRelativeTo(e, e.target.closest('.vp--visual-node'));
                this.nodeDragData = { element: e.target.closest('.vp--visual-node'), offset };
            }
            else if (e.target.classList.contains('vp--node-link')) {
                e.preventDefault();
                this.connectorDragStartData = this.getDragDataFromNode(e.target);
                let svgOffset = VisualProgrammer.getMousePositionRelativeTo(e, this.lineContainer);
                this.connectorDragStartPos = Object.assign(Object.assign({}, svgOffset), { isVert: e.target.dataset.nodeLinkType == "statement" });
                this.drawPathFrom(this.previewLine, this.connectorDragStartPos, this.connectorDragStartPos, this.connectorDragStartPos.isVert);
                this.previewLine.style.display = "block";
            }
        };
        this.documentOnPointerMove = (e) => {
            // This is added to the document rather than the using the pointer capture method (as with the device-preview script) because
            // the pointerup event needs to capture the element (connector) which the user's mouse was over at release. This is ALWAYS the
            // element that has capture if the capture has been sent.
            if (this.nodeDragData != null && this.nodeDragData.element != null) {
                let r = VisualProgrammer.getMousePositionRelativeTo(e, this.nodeContainer);
                this.nodeDragData.element.parentElement.style.left = Math.round(r.x - this.nodeDragData.offset.x) + "px";
                this.nodeDragData.element.parentElement.style.top = Math.round(r.y - this.nodeDragData.offset.y) + "px";
                this.updateLinePositions();
            }
            else if (this.connectorDragStartData != null) {
                let mousePos = VisualProgrammer.getMousePositionRelativeTo(e, this.lineContainer);
                this.drawPathFrom(this.previewLine, this.connectorDragStartPos, mousePos, this.connectorDragStartPos.isVert);
            }
        };
        this.onPointerUp = (e) => {
            if (this.nodeDragData != null) {
                let { x, y } = VisualProgrammer.getMousePositionRelativeTo(e, this.nodeContainer);
                this.dotNet.invokeMethodAsync("SetPosition", this.nodeDragData.element.dataset.visualNodeId, Math.round(x - this.nodeDragData.offset.x), Math.round(y - this.nodeDragData.offset.y));
            }
            else if (this.connectorDragStartData != null && e.target instanceof HTMLElement && e.target.classList.contains('vp--node-link')) {
                this.dotNet.invokeMethodAsync("SetLink", this.connectorDragStartData, this.getDragDataFromNode(e.target));
            }
            this.nodeDragData = this.connectorDragStartData = null;
            this.previewLine.style.display = "none";
        };
        this.nodeContainer = element.querySelector('.vp--node-container');
        this.lineContainer = element.querySelector('.vp--node-connector-container');
        this.previewLine = element.querySelector('.vp--preview-line');
        element.addEventListener('pointerdown', this.onPointerDown);
        document.addEventListener('pointermove', this.documentOnPointerMove);
        element.addEventListener('pointerup', this.onPointerUp);
        this.updateLinePositions();
        element.__visualProgrammer = this;
    }
    static init(element, dotNet) {
        new VisualProgrammer(element, dotNet);
    }
    updateLinePositions() {
        [...this.lineContainer.querySelectorAll('path')].forEach(path => {
            if (path.dataset.lineDestId != null && path.dataset.lineDestId != "") {
                let sourceEl = this.element.querySelector(`[data-visual-node-id="${path.dataset.lineSourceId}"] [data-node-link-role="source"][data-node-link-name="${path.dataset.lineSourceName}"]`);
                let destEl = this.element.querySelector(`[data-visual-node-id="${path.dataset.lineDestId}"] [data-node-link-role="destination"]`);
                let sourcePos = VisualProgrammer.getOffsetRelativeTo(sourceEl, this.nodeContainer), destPos = VisualProgrammer.getOffsetRelativeTo(destEl, this.nodeContainer);
                this.drawPathFrom(path, sourcePos, destPos, sourceEl.dataset.nodeLinkType == "statement");
            }
        });
    }
    static onNewNode(element, id) {
        var _a;
        let vp = element.__visualProgrammer;
        // We need to wait for an afterRender from Blazor, otherwise the element won't exist yet.
        (_a = vp) === null || _a === void 0 ? void 0 : _a.awaitRender().then(() => vp.nodeDragData = {
            element: vp.element.querySelector(`[data-visual-node-id="${id}"]`),
            offset: { x: 10, y: 10 }
        });
    }
    drawPathFrom(path, p1, p2, isVert) {
        // Calculate the two Bezier control point coordinates
        let cpx1 = isVert ? p1.x : ((p1.x + p2.x) / 2), cpy1 = isVert ? ((p1.y + p2.y) / 2) : p1.y, cpx2 = isVert ? p2.x : ((p1.x + p2.x) / 2), cpy2 = isVert ? ((p1.y + p2.y) / 2) : p2.y;
        path.setAttribute('d', `M ${p1.x} ${p1.y} C ${cpx1} ${cpy1}, ${cpx2} ${cpy2}, ${p2.x} ${p2.y}`);
    }
    getDragDataFromNode(connector) {
        return {
            id: connector.closest('[data-visual-node-id]').dataset.visualNodeId,
            type: connector.dataset.nodeLinkType,
            name: connector.dataset.nodeLinkName,
            role: connector.dataset.nodeLinkRole
        };
    }
    static getMousePositionRelativeTo(evt, element) {
        let bounds = element.getBoundingClientRect();
        return { x: evt.clientX - bounds.left, y: evt.clientY - bounds.top };
    }
    static getOffsetRelativeTo(el1, el2) {
        let r1 = el1.getBoundingClientRect(), r2 = el2.getBoundingClientRect();
        return { x: r1.left - r2.left, y: r1.top - r2.top };
    }
    static afterRender(element) {
        var _a, _b, _c, _d;
        (_a = element.__visualProgrammer) === null || _a === void 0 ? void 0 : _a.updateLinePositions();
        (_d = (_b = element.__visualProgrammer) === null || _b === void 0 ? void 0 : (_c = _b).awaitRenderResolve) === null || _d === void 0 ? void 0 : _d.call(_c);
    }
    awaitRender() {
        var _a;
        return _a = this.awaitRenderPromise, (_a !== null && _a !== void 0 ? _a : (this.awaitRenderPromise = new Promise(resolve => this.awaitRenderResolve = () => {
            resolve();
            this.awaitRenderPromise = this.awaitRenderResolve = null;
        })));
    }
    dispose() {
        this.element.removeEventListener('pointerdown', this.onPointerDown);
        document.removeEventListener('pointermove', this.documentOnPointerMove);
        this.element.removeEventListener('pointerup', this.onPointerUp);
        delete this.element.__visualProgrammer;
    }
    static dispose(element) {
        var _a;
        (_a = element.__visualProgrammer) === null || _a === void 0 ? void 0 : _a.dispose();
    }
}
window.VisualProgrammer = VisualProgrammer;
//# sourceMappingURL=script.js.map