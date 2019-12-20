type ConnectorData = { id: string, type: string, name: string, role: string };
type Point = { x: number, y: number };
type DotNetReference = { invokeMethodAsync: (method: string, ...params: any[]) => any };
interface HTMLElement { __visualProgrammer?: VisualProgrammer; }


class VisualProgrammer {

    private nodeContainer: HTMLDivElement
    private lineContainer: SVGElement;
    private previewLine: SVGPathElement;

    private nodeDragData?: { element: HTMLElement, offset: Point };
    private connectorDragStartData?: ConnectorData;
    private connectorDragStartPos?: Point & { isVert: boolean };

    private awaitRenderPromise: Promise<void>;
    private awaitRenderResolve: () => void;

    public static init(element: HTMLElement, dotNet: DotNetReference) {
        new VisualProgrammer(element, dotNet);
    }

    public constructor(private element: HTMLElement, private dotNet: DotNetReference) {
        this.nodeContainer = element.querySelector('.vp--node-container');
        this.lineContainer = element.querySelector('.vp--node-connector-container');
        this.previewLine = element.querySelector('.vp--preview-line');

        element.addEventListener('pointerdown', this.onPointerDown);
        document.addEventListener('pointermove', this.documentOnPointerMove);
        element.addEventListener('pointerup', this.onPointerUp);

        this.updateLinePositions();

        element.__visualProgrammer = this;
    }

    private onPointerDown = (e: PointerEvent) => {
        if (!(e.target instanceof Element)) return;

        // If the user clicks down on the part of the visual node marked as the dragger, we want to start moving that node.
        if (e.target.classList.contains('vp--node-dragger')) {
            // No longer doing e.preventDefault() here, since we want it to bubble up to be cause by the handler in VisualProgramEditor.razor so the node can be selected
            let offset = VisualProgrammer.getMousePositionRelativeTo(e, e.target.closest('.vp--visual-node'));
            this.nodeDragData = { element: e.target.closest('.vp--visual-node'), offset };

        } else if (e.target.classList.contains('vp--node-link')) {
            e.preventDefault();
            this.connectorDragStartData = this.getDragDataFromNode(<HTMLElement>e.target);
            let svgOffset = this.getConnectorPosition(<HTMLElement>e.target);
            this.connectorDragStartPos = { ...svgOffset, isVert: (<any>e.target).dataset.nodeLinkType == "statement" };
            this.drawPathFrom(this.previewLine, this.connectorDragStartPos, this.connectorDragStartPos, this.connectorDragStartPos.isVert);
            this.previewLine.style.display = "block";
            // Set the preview line 'data-type' attribute to either be the type of node being dragged, or in the case of an expression output, the type of the expression
            this.previewLine.dataset.type = (<HTMLElement>e.target).dataset.type ?? (<HTMLElement>e.target.closest('.vp--visual-node')).dataset.type ?? "";
        }
    };

    private documentOnPointerMove = (e: PointerEvent) => {
        // This is added to the document rather than the using the pointer capture method (as with the device-preview script) because
        // the pointerup event needs to capture the element (connector) which the user's mouse was over at release. This is ALWAYS the
        // element that has capture if the capture has been sent.

        if (this.nodeDragData != null && this.nodeDragData.element != null) {
            let r = VisualProgrammer.getMousePositionRelativeTo(e, this.nodeContainer);
            this.nodeDragData.element.parentElement.style.left = Math.round(r.x - this.nodeDragData.offset.x) + "px";
            this.nodeDragData.element.parentElement.style.top = Math.round(r.y - this.nodeDragData.offset.y) + "px";
            this.updateLinePositions();

        } else if (this.connectorDragStartData != null) {
            let mousePos = VisualProgrammer.getMousePositionRelativeTo(e, this.lineContainer);
            this.drawPathFrom(this.previewLine, this.connectorDragStartPos, mousePos, this.connectorDragStartPos.isVert);
        }
    }

    private onPointerUp = (e: PointerEvent) => {
        if (this.nodeDragData != null) {
            let { x, y } = VisualProgrammer.getMousePositionRelativeTo(e, this.nodeContainer);
            this.dotNet.invokeMethodAsync("SetPosition", this.nodeDragData.element.dataset.visualNodeId, Math.round(x - this.nodeDragData.offset.x), Math.round(y - this.nodeDragData.offset.y));

        } else if (this.connectorDragStartData != null && e.target instanceof HTMLElement && e.target.classList.contains('vp--node-link')) {
            this.dotNet.invokeMethodAsync("SetLink", this.connectorDragStartData, this.getDragDataFromNode(e.target));
        }
        this.nodeDragData = this.connectorDragStartData = null;
        this.previewLine.style.display = "none";
    }

    public updateLinePositions() {
        (<SVGPathElement[]>Array.from(this.lineContainer.querySelectorAll('path'))).forEach(path => {
            if (path.dataset.lineDestId != null && path.dataset.lineDestId != "") {
                let sourceEl = this.element.querySelector(`[data-visual-node-id="${path.dataset.lineSourceId}"] [data-node-link-role="source"][data-node-link-name="${path.dataset.lineSourceName}"]`);
                let destEl = this.element.querySelector(`[data-visual-node-id="${path.dataset.lineDestId}"] [data-node-link-role="destination"]`);
                let sourcePos = this.getConnectorPosition(sourceEl), destPos = this.getConnectorPosition(destEl);
                this.drawPathFrom(path, sourcePos, destPos, (<HTMLElement>sourceEl).dataset.nodeLinkType == "statement");
            }
        });
    }

    public static onNewNode(element: HTMLElement, id: string) {
        let vp = element.__visualProgrammer;
        // We need to wait for an afterRender from Blazor, otherwise the element won't exist yet.
        vp?.awaitRender().then(() =>
            vp.nodeDragData = {
                element: vp.element.querySelector(`[data-visual-node-id="${id}"]`),
                offset: { x: 10, y: 10 }
            }
        );
    }

    private drawPathFrom(path: SVGPathElement, p1: Point, p2: Point, isVert: boolean) {
        // Calculate the two Bezier control point coordinates
        let cpx1 = isVert ? p1.x : ((p1.x + p2.x) / 2),
            cpy1 = isVert ? ((p1.y + p2.y) / 2) : p1.y,
            cpx2 = isVert ? p2.x : ((p1.x + p2.x) / 2),
            cpy2 = isVert ? ((p1.y + p2.y) / 2) : p2.y;
        path.setAttribute('d', `M ${p1.x} ${p1.y} C ${cpx1} ${cpy1}, ${cpx2} ${cpy2}, ${p2.x} ${p2.y}`);
    }

    private getDragDataFromNode(connector: HTMLElement): ConnectorData {
        return {
            id: (<HTMLElement>connector.closest('[data-visual-node-id]')).dataset.visualNodeId,
            type: connector.dataset.nodeLinkType,
            name: connector.dataset.nodeLinkName,
            role: connector.dataset.nodeLinkRole
        };
    }

    private static getMousePositionRelativeTo(evt: MouseEvent, element: Element): Point {
        let bounds = element.getBoundingClientRect();
        return { x: evt.clientX - bounds.left, y: evt.clientY - bounds.top };
    }

    private static getOffsetRelativeTo(el1: Element, el2: Element): Point {
        let r1 = el1.getBoundingClientRect(), r2 = el2.getBoundingClientRect();
        return { x: r1.left - r2.left, y: r1.top - r2.top };
    }

    private getConnectorPosition(connector: Element): Point {
        return VisualProgrammer.getOffsetRelativeTo(connector, this.nodeContainer);
    }

    public static afterRender(element: HTMLElement) {
        element.__visualProgrammer?.updateLinePositions();
        element.__visualProgrammer?.awaitRenderResolve?.();
    }

    private awaitRender() {
        return this.awaitRenderPromise ?? (this.awaitRenderPromise = new Promise<void>(resolve =>
            this.awaitRenderResolve = () => {
                resolve();
                this.awaitRenderPromise = this.awaitRenderResolve = null;
            }
        ));
    }

    public dispose() {
        this.element.removeEventListener('pointerdown', this.onPointerDown);
        document.removeEventListener('pointermove', this.documentOnPointerMove);
        this.element.removeEventListener('pointerup', this.onPointerUp);
        delete this.element.__visualProgrammer;
    }

    public static dispose(element: HTMLElement) {
        element.__visualProgrammer?.dispose();
    }
}
(<any>window).VisualProgrammer = VisualProgrammer;
