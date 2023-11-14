function initializeSlider(element, min, max, dotNetInstance) {
    var slider = noUiSlider.create(element, {
        start: [min, max],
        connect: true,
        range: {
            'min': min,
            'max': max
        }
    });

    slider.on('update', function (values, handle) {
        const intValue1 = parseInt(values[0]);
        const intValue2 = parseInt(values[1]);
        dotNetInstance.invokeMethodAsync("OnSliderValueChanged", intValue1, intValue2);
    });
}