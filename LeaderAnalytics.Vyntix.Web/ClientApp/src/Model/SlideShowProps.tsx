import DialogType from './DialogType';

export default class SlideShowProps {
    public Images: string[] = new Array(1);
    public DialogType: DialogType = DialogType.Info;
    public Callback: any = () => { };

    constructor(images: string[], dialogType: DialogType, callBack: Function) {
        this.Images = images;
        this.DialogType = dialogType
        this.Callback = callBack;
    }
}
