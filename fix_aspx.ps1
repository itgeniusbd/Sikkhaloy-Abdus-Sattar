$missingContent = @'

                        if ($headerCells.length > cellIndex) {
                            columnHeader = $headerCells.eq(cellIndex).text().trim();
                        }

                        if (cellText === 'A') {
                            if (columnHeader === 'Grade' || columnHeader.indexOf('Grade') !== -1) {
                                return;
                            }

                            if (columnHeader === 'Obtain Marks' ||
                                columnHeader.indexOf('Number') !== -1 ||
                                columnHeader === 'Midterm' ||
                                columnHeader === 'Periodical' ||
                                columnHeader === 'Subjective' ||
                                columnHeader === 'Objective' ||
                                cellIndex <= 2) {

                                $cell.text('Abs');
                            }
                        }
                        else if (cellText === '0' && $cell.hasClass('total-marks-cell')) {
                            var hasAbsentMarks = false;
                            $row.find('td').each(function () {
                                var siblingText = $(this).text().trim();
                                if (siblingText === 'Abs') {
                                    hasAbsentMarks = true;
                                    return false;
                                }
                            });

                            if (hasAbsentMarks) {
                                $cell.text('-');
                            }
                        }
                    });
                });
            });

            if ($('#NumberToggleButton').length > 0) {
                $('#NumberToggleButton').html('Bengali Numbers').removeClass('btn-info').addClass('btn-warning');
                isNumbersBengali = false;
            }
        }
    </script>
</asp:Content>
'@

$filePath = "F:\SIKKHALOY-V3\SIKKHALOY V2\Exam\CumulativeResult\CumulativeResultCardt.aspx"
Add-Content -Path $filePath -Value $missingContent -Encoding UTF8
Write-Host "Missing content added successfully!"
