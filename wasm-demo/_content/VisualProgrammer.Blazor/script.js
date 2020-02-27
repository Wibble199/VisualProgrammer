class VisualProgrammer {
    constructor(element, dotNet) {
        this.element = element;
        this.dotNet = dotNet;
        this.selectedNodes = new Set();
        this.onPointerDown = (e) => {
            var _a, _b;
            if (!(e.target instanceof Element))
                return;
            let closestNode = e.target.closest('.vp--visual-node');
            // If the user clicks down on the part of the visual node marked as the dragger, we want to start moving that node.
            if (e.target.classList.contains('vp--node-dragger')) {
                this.doNodeSelection(closestNode.dataset.visualNodeId, e.ctrlKey || e.shiftKey);
                this.nodeDragData = [];
                for (let nodeId of this.selectedNodes) {
                    let node = document.querySelector(`[data-visual-node-id="${nodeId}"]`);
                    this.nodeDragData.push({ element: node, offset: VisualProgrammer.getMousePositionRelativeTo(e, node) });
                }
                // If the user clicked down on a node link part of a node
            }
            else if (e.target.classList.contains('vp--node-link')) {
                e.preventDefault();
                this.connectorDragStartData = this.getDragDataFromNode(e.target);
                let svgOffset = this.getConnectorPosition(e.target);
                this.connectorDragStartPos = Object.assign(Object.assign({}, svgOffset), { isVert: e.target.dataset.nodeLinkType == "statement" });
                this.drawPathFrom(this.previewLine, this.connectorDragStartPos, this.connectorDragStartPos, this.connectorDragStartPos.isVert);
                this.previewLine.style.display = "block";
                // Set the preview line 'data-type' attribute to either be the type of node being dragged, or in the case of an expression output, the type of the expression
                this.previewLine.dataset.type = (_b = (_a = e.target.dataset.type, (_a !== null && _a !== void 0 ? _a : closestNode.dataset.type)), (_b !== null && _b !== void 0 ? _b : ""));
                // Otherwise, if the mouse went down on any element inside a visual node (including inputs etc., but not the dragger or the node links)
            }
            else if (closestNode != null) {
                this.doNodeSelection(closestNode.dataset.visualNodeId, e.ctrlKey || e.shiftKey);
                // Otherwise if the mouse went down anywhere else on the canvas
            }
            else if (e.target == this.canvasContainer) {
                // Deselect all nodes
                this.clearSelection();
            }
        };
        this.documentOnPointerMove = (e) => {
            // This is added to the document rather than the using the pointer capture method (as with the device-preview script) because
            // the pointerup event needs to capture the element (connector) which the user's mouse was over at release. This is ALWAYS the
            // element that has capture if the capture has been sent.
            if (this.nodeDragData != null) {
                let r = VisualProgrammer.getMousePositionRelativeTo(e, this.nodeContainer);
                for (let { element, offset } of this.nodeDragData) {
                    element.parentElement.style.left = Math.round(r.x - offset.x) + "px";
                    element.parentElement.style.top = Math.round(r.y - offset.y) + "px";
                }
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
                for (let { element, offset } of this.nodeDragData)
                    this.dotNet.invokeMethodAsync("SetPosition", element.dataset.visualNodeId, Math.round(x - offset.x), Math.round(y - offset.y));
            }
            else if (this.connectorDragStartData != null && e.target instanceof HTMLElement && e.target.classList.contains('vp--node-link')) {
                this.dotNet.invokeMethodAsync("SetLink", this.connectorDragStartData, this.getDragDataFromNode(e.target));
            }
            this.nodeDragData = this.connectorDragStartData = null;
            this.previewLine.style.display = "none";
        };
        this.documentOnKeyDown = (e) => {
            if (e.keyCode == 46 && this.selectedNodes.size) { // 46 = delete (but not backspace)
                // Send an array of node IDs to Blazor to have the nodes removed
                let nodeIds = [...this.selectedNodes];
                this.clearSelection(); // This needs to be called because otherwise Blazor will reuse some deleted nodes instead which will end up appearing selected even though they shouln't be
                this.dotNet.invokeMethodAsync("DeleteNodes", nodeIds);
            }
        };
        this.canvasContainer = element.querySelector('.vp--canvas');
        this.nodeContainer = element.querySelector('.vp--node-container');
        this.lineContainer = element.querySelector('.vp--node-connector-container');
        this.previewLine = element.querySelector('.vp--preview-line');
        element.addEventListener('pointerdown', this.onPointerDown);
        document.addEventListener('pointermove', this.documentOnPointerMove);
        element.addEventListener('pointerup', this.onPointerUp);
        document.addEventListener('keydown', this.documentOnKeyDown);
        this.updateLinePositions();
        element.__visualProgrammer = this;
    }
    static init(element, dotNet) {
        new VisualProgrammer(element, dotNet);
    }
    updateLinePositions() {
        Array.from(this.lineContainer.querySelectorAll('path')).forEach(path => {
            if (path.dataset.lineDestId != null && path.dataset.lineDestId != "") {
                let sourceEl = this.element.querySelector(`[data-visual-node-id="${path.dataset.lineSourceId}"] [data-node-link-role="source"][data-node-link-name="${path.dataset.lineSourceName}"]`);
                let destEl = this.element.querySelector(`[data-visual-node-id="${path.dataset.lineDestId}"] [data-node-link-role="destination"]`);
                let sourcePos = this.getConnectorPosition(sourceEl), destPos = this.getConnectorPosition(destEl);
                this.drawPathFrom(path, sourcePos, destPos, sourceEl.dataset.nodeLinkType == "statement");
            }
        });
    }
    static onNewNode(element, id) {
        var _a;
        let vp = element.__visualProgrammer;
        // We need to wait for an afterRender from Blazor, otherwise the element won't exist yet.
        (_a = vp) === null || _a === void 0 ? void 0 : _a.awaitRender().then(() => {
            vp.doNodeSelection(id, false);
            vp.nodeDragData = [{
                    element: vp.element.querySelector(`[data-visual-node-id="${id}"]`),
                    offset: { x: 10, y: 10 }
                }];
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
    /** Sets whether or not the node with the given ID is selected or not.
     * @param newStatus Whether or not the node should be selected. Leave blank to toggle the selection on/off. */
    setNodeSelected(id, newStatus = null) {
        newStatus = (newStatus !== null && newStatus !== void 0 ? newStatus : !this.selectedNodes.has(id));
        this.selectedNodes[newStatus ? "add" : "delete"](id);
        document.querySelector(`[data-visual-node-id="${id}"]`).closest('.vp--visual-node-container').classList[newStatus ? "add" : "remove"]('vp--selected');
    }
    doNodeSelection(id, ctrlOrShift) {
        if (ctrlOrShift)
            this.setNodeSelected(id); // If ctrl or shift, toggle this node to to selection
        else {
            this.clearSelection(); // Otherwise, only select this node
            this.setNodeSelected(id, true);
        }
    }
    clearSelection() {
        this.selectedNodes.clear();
        Array.from(document.querySelectorAll('.vp--visual-node-container.vp--selected')).forEach(el => el.classList.remove('vp--selected'));
    }
    static getMousePositionRelativeTo(evt, element) {
        let bounds = element.getBoundingClientRect();
        return { x: evt.clientX - bounds.left, y: evt.clientY - bounds.top };
    }
    static getOffsetRelativeTo(el1, el2) {
        let r1 = el1.getBoundingClientRect(), r2 = el2.getBoundingClientRect();
        return { x: r1.left - r2.left, y: r1.top - r2.top };
    }
    getConnectorPosition(connector) {
        return VisualProgrammer.getOffsetRelativeTo(connector, this.nodeContainer);
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